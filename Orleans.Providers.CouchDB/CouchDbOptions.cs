using System;
using System.Net;
using Orleans.Providers.CouchDB.Client.Auth;
using Orleans.Providers.CouchDB.Serialization;
using Orleans.Runtime;

namespace Orleans.Providers.CouchDB
{
	public class CouchDbOptions
	{
        public string? EndPoint { get; set; }

        public ICouchDbAuth? Authentication { get; set; }

        public ICouchDbDocumentSerializer? DocumentSerializer { get; set; }

        internal virtual void Validate()
        {
            if (string.IsNullOrEmpty(EndPoint) || string.IsNullOrEmpty(EndPoint))
            {
                throw new OrleansConfigurationException($"Invalid values for {nameof(EndPoint)}. {nameof(EndPoint)} is required.");
            }
            if (Authentication ==null )
            {
                throw new OrleansConfigurationException($"Invalid values for {nameof(Authentication)}. {nameof(Authentication)} is required.");
            }
        }
    }


}

