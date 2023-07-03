using System;
namespace Orleans.Providers.CouchDB.Client.Auth
{
	public interface ICouchDbAuth
	{
        Task<HttpClient> Authenticate(string endpoint, IHttpClientFactory factory);
        Task PostRequest(string endpoint, HttpResponseMessage response);
    }
}

