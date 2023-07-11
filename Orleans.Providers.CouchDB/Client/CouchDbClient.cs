using System;
using System.Net.Http;
using System.Text.Json;
using System.Web;
using Orleans.Providers.CouchDB.Client.Auth;
using Orleans.Providers.CouchDB.Client.Changes;
using Orleans.Providers.CouchDB.Serialization;

namespace Orleans.Providers.CouchDB.Client
{
    public interface ICouchDbClient
    {
        Task<CouchDbPutResponse?> Put<T>(string db, string id, T doc, string? rev);

        Task<CouchDbGetResponse<T>?> Get<T>(string db, string id, string? rev);

        Task<CouchDbPutResponse?> Delete(string db, string id, string rev);

        Task EnsureDatabase(string db);

        IAsyncEnumerable<CouchDbChangesResponse> Changes(string db, CouchDbChangesOptions options, CancellationToken cancellationToken);


    }

    public class CouchDbClient : ICouchDbClient
    {
        private readonly string endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICouchDbAuth _authentication;
        private readonly ICouchDbDocumentSerializer _serializer;

        public CouchDbClient(
            string? endpoint,
            IHttpClientFactory httpClientFactory,
            ICouchDbAuth? authentication,
            ICouchDbDocumentSerializer? serializer = null)
		{
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));

            this.endpoint = endpoint;
            _httpClientFactory = httpClientFactory;
            _authentication = authentication;
            _serializer = serializer ?? new CouchDbDefaultDocumentSerializer();
        }


        Task<HttpClient> GetClient()
        {
            return _authentication.Authenticate(endpoint, _httpClientFactory);
        }

        string GetUrl(string url, string? rev)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            if (rev != null && !string.IsNullOrEmpty(rev) && !string.IsNullOrWhiteSpace(rev))
            {
                queryParams.Add("rev", rev);
            }

            var builder = new UriBuilder(url);
            builder.Query = queryParams.ToString();
            return builder.ToString();
        }

        

        public async Task<CouchDbPutResponse?> Put<T>(string db, string id, T doc, string? rev)
        {
            var json = _serializer.Serialize(doc);
            var url = GetUrl($"{endpoint}/{db}/{id}", rev);
           
            var client = await GetClient();
            var response = await client.PutAsync(url, new StringContent(json));
            await _authentication.PostRequest(endpoint, response);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw CouchDbException.FromBody(body);
            }

            return JsonSerializer.Deserialize<CouchDbPutResponse>(body);

        }

        public async Task<CouchDbGetResponse<T>?> Get<T>(string db, string id, string? rev)
        {
            var url = GetUrl($"{endpoint}/{db}/{id}", rev);
            var client = await GetClient();
            var response = await client.GetAsync(url);
            await _authentication.PostRequest(endpoint, response);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
               

                var doc = _serializer.Deserialize<T>(body);
                var etag = response.Headers.ETag?.Tag.Replace("\"", "").Trim();
               
                return new CouchDbGetResponse<T>()
                {
                    Id = id,
                    Doc = doc,
                    Rev = etag
                };
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            throw CouchDbException.FromBody(body);
        }

        public async Task<CouchDbPutResponse?> Delete(string db, string id, string rev)
        {
            var url = $"{endpoint}/{db}/{id}";
            var client = await GetClient();
            var response = await client.DeleteAsync(url);
            await _authentication.PostRequest(endpoint, response);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw CouchDbException.FromBody(body);
            return JsonSerializer.Deserialize<CouchDbPutResponse>(body);
        }

        public async Task EnsureDatabase(string db)
        {
            var url = $"{endpoint}/{db}";
            var client = await GetClient();
            var response = await client.PutAsync(url, new StringContent(""));
            await _authentication.PostRequest(endpoint, response);
            if (response.IsSuccessStatusCode) return;
            if (response.StatusCode == System.Net.HttpStatusCode.PreconditionFailed) return;
            throw CouchDbException.FromBody(await response.Content.ReadAsStringAsync());
        }

        public IAsyncEnumerable<CouchDbChangesResponse> Changes(string db, CouchDbChangesOptions options, CancellationToken cancellationToken)
        {
            return (new CouchDbChangesFeedStream(this.endpoint, db, this._httpClientFactory, _authentication, options)).Changes(cancellationToken);
        }
    }
}

