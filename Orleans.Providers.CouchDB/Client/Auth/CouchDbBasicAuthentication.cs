using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Orleans.Providers.CouchDB.Client.Auth
{
    public class CouchDbBasicAuthentication : AuthenticationHeaderValue, ICouchDbAuth
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CouchDbBasicAuthentication"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public CouchDbBasicAuthentication(string userName, string password)
            : base("Basic", EncodeCredential(userName, password))
        { }

        /// <summary>
        /// Encodes the credential.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">userName</exception>
        private static string EncodeCredential(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
            if (password == null) password = "";

            Encoding encoding = Encoding.UTF8;
            string credential = String.Format("{0}:{1}", userName, password);

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }

        public Task<HttpClient> Authenticate(string endpoint, IHttpClientFactory factory)
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = this;
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return Task.FromResult(client);
        }

        public Task PostRequest(string endpoint, HttpResponseMessage response) => Task.CompletedTask;
        
    }
}

