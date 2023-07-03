using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace Orleans.Providers.CouchDB.Client.Auth
{
	public class CouchDbSessionAuthentication : ICouchDbAuth
    {
        private readonly string userName;
        private readonly string password;
        private readonly TimeSpan? minDuration;
        private readonly Cookie? cookie;

        private CookieContainer cookies = new CookieContainer();

        private SemaphoreSlim asyncLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="CouchDbSessionAuthentication"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public CouchDbSessionAuthentication(string userName, string password, TimeSpan? minDuration = null)
        {
            this.userName = userName;
            this.password = password;
            this.minDuration = minDuration;
        }

        private bool Expired(Cookie cookie)
        {
            var m_expires = cookie.Expires;
            var now = DateTime.Now;
            if (m_expires >= now) return true;

            var rem = m_expires.ToLocalTime() - now;

            return rem < (minDuration ?? TimeSpan.FromMinutes(2));
        }

        private async Task GetCookies(string endpoint, IHttpClientFactory factory)
        {
            cookies = new CookieContainer();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            var credentials = new
            {
                name = userName,
                password = password
            };

            var response =
                await client.PostAsync($"{endpoint}/_session", new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8,"application/json"));

            if(!response.IsSuccessStatusCode)
            {
                throw CouchDbException.FromBody(await response.Content.ReadAsStringAsync());
            }

            var cookiesStr = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            if(cookiesStr != null && cookiesStr.Any()) {
                foreach (var cookieStr in cookiesStr)
                {
                    cookies.SetCookies(new Uri(endpoint), cookieStr);
                }
            }
        }

        public async Task<HttpClient> Authenticate(string endpoint, IHttpClientFactory factory)
        {
            await asyncLock.WaitAsync();
            try
            {
                var expired = cookies.Count == 0 || cookies.GetAllCookies().Any(c => Expired(c));
                if (expired)
                {
                    await GetCookies(endpoint, factory);
                }

                var client = factory.CreateClient();
                client.DefaultRequestHeaders.Add("Cookie", cookies.GetCookieHeader(new Uri(endpoint)));
                return client;
            }
            finally
            {
                asyncLock.Release();
            }

        }

        public async Task PostRequest(string endpoint, HttpResponseMessage response)
        {
            await asyncLock.WaitAsync();
            try
            {
                var cookiesStr = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                if (cookiesStr != null && cookiesStr.Any())
                {
                    cookies = new CookieContainer();
                    foreach (var cookieStr in cookiesStr)
                    {
                        cookies.SetCookies(new Uri(endpoint), cookieStr);
                    }
                }
            }
            finally
            {
                asyncLock.Release();
            }
        }
    }
}

