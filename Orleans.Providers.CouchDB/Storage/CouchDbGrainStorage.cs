using System;
using Microsoft.Extensions.Logging;
using Orleans.Providers.CouchDB.Client;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Providers.CouchDB.Storage
{
    public class CouchDbGrainStorage : IGrainStorage
    {
        private readonly ICouchDbClient couchDbClient;
        private readonly ILogger<CouchDbGrainStorage> logger;
        private readonly CouchDbOptions options;
        private bool databaseCreated = false;

        public CouchDbGrainStorage(
            IHttpClientFactory factory,
            ILogger<CouchDbGrainStorage> logger,
            CouchDbOptions options)
        {
            this.couchDbClient = new CouchDbClient(
                    endpoint: options.EndPoint,
                    httpClientFactory: factory,
                    authentication: options.Authentication,
                    serializer: options.DocumentSerializer
                );
            this.logger = logger;
            this.options = options;
        }
        /*
        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe<CouchDbGrainStorage>(ServiceLifecycleStage.ApplicationServices, Init);
        }

        private Task Init(CancellationToken ct)
        {
            return DoAndLog(nameof(Init), () =>
            {
                options.Validate();
                return couchDbClient.EnsureDatabase(options.Database);
            });
        }

        
        */

        async ValueTask EnsureDatabase(string stateName)
        {
            if (databaseCreated) return;
            await couchDbClient.EnsureDatabase(stateName);
            databaseCreated = true;
        }

        private string ParseId(string id)
        {
            if (id.Contains("/"))
            {
                return id.Split("/")[1];
            }
            return id;
        }

       

        public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            await EnsureDatabase(stateName);
            var id = ParseId(grainId.ToString());
            var response = await couchDbClient.Delete(stateName, id, grainState.ETag);
            if (response == null) return;

            if (response.Ok && response.Id == id)
            {
                grainState.RecordExists = false;
            }
        }

        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            await EnsureDatabase(stateName);
            var id = ParseId(grainId.ToString());
            grainState.RecordExists = false;
           var response =  await couchDbClient.Get<T>(stateName, id, grainState.ETag);
           
            if (response != null)
            {
                if (response.Doc != null)
                {
                    grainState.State = response.Doc;
                    grainState.RecordExists = true;
                }
                grainState.ETag = response.Rev;
               
            }
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            await EnsureDatabase(stateName);
            var id = ParseId(grainId.ToString());
            grainState.RecordExists = false;
            var response = await couchDbClient.Put(stateName, id, grainState.State, grainState.ETag);
            if (response != null)
            {
                if (response.Ok && id == response.Id)
                {
                    grainState.RecordExists = true;
                    grainState.ETag = response.Rev;
                }
            }
        }

        private Task DoAndLog(string actionName, Func<Task> action)
        {
            return DoAndLog(actionName, async () => { await action(); return true; });
        }

        private async Task<T> DoAndLog<T>(string actionName, Func<Task<T>> action)
        {
            logger.LogDebug($"{nameof(CouchDbGrainStorage)}.{actionName} called.");

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                logger.LogError((int)CouchDbProviderErrorCode.GrainStorageOperations, ex, $"{nameof(CouchDbGrainStorage)}.{actionName} failed. Exception={ex.Message}");

                throw;
            }
        }
    }
}

