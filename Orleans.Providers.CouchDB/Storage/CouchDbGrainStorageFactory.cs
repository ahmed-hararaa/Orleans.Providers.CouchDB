using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;

namespace Orleans.Providers.CouchDB.Storage
{
    
	public static class CouchDbGrainStorageFactory
	{
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<CouchDbGrainStorageOptions>>();

            return ActivatorUtilities.CreateInstance<CouchDbGrainStorage>(services, optionsMonitor.Get(name));
        }
    }
    
}

