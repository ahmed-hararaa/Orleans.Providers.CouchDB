using System;
using Orleans.Providers.CouchDB.Client.Auth;

namespace Orleans.Providers.CouchDB.Client
{
	public class EventSourceClient
	{
        private readonly IHttpClientFactory factory;

        public EventSourceClient(IHttpClientFactory factory, string endpoint, ICouchDbAuth auth)
		{
            this.factory = factory;
        }


	}
}

