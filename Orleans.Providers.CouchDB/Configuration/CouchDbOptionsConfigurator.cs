using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers.CouchDB.Serialization;
using Orleans.Runtime;

namespace Orleans.Providers.CouchDB.Configuration
{
    public class CouchDbOptionsConfigurator : IPostConfigureOptions<CouchDbOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStateProviderSerializerOptionsConfigurator{TOptions}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public CouchDbOptionsConfigurator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

       
        public void PostConfigure(string? name, CouchDbOptions options)
        {
          
        }
    }
}

