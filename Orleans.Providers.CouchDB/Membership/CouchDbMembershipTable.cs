using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers.CouchDB.Client;
using Orleans.Runtime;

namespace Orleans.Providers.CouchDB.Membership
{
	public class CouchDbMembershipTable : IMembershipTable
    {
        private string couchDbMembershipTable;

        private readonly ICouchDbClient couchdb;
        private readonly ILogger<CouchDbMembershipTable> logger;
        private readonly string clusterId;
        private static readonly TableVersion NotFound = new TableVersion(0, "0");

        public CouchDbMembershipTable(
            IHttpClientFactory factory,
            IOptions<CouchDbMembershipOptions> membershipOptions,
            IOptions<ClusterOptions> clusterOptions,
            ILogger<CouchDbMembershipTable> logger
            )
		{
            var options = membershipOptions.Value;
            this.couchdb = new CouchDbClient(
                    endpoint: options.EndPoint,
                    httpClientFactory: factory,
                    authentication: options.Authentication,
                    serializer: options.DocumentSerializer
                );
            this.logger = logger;
            this.clusterId = clusterOptions.Value.ClusterId;
            couchDbMembershipTable = options.DatabaseName ?? "couchdb_orleans_membership";
        }

        public Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            return DoAndLog(nameof(CleanupDefunctSiloEntries), () =>
            {
                return CleanupDefunctSiloEntries(clusterId, beforeDate);
            });
        }

        public Task DeleteMembershipTableEntries(string deploymentId)
        {
            return DoAndLog(nameof(DeleteMembershipTableEntries), async () =>
            {
                var (rev, deployment) = await GetDeployment(deploymentId);
                await couchdb.Delete(couchDbMembershipTable, deploymentId, rev);
            });
        }

        public Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            return couchdb.EnsureDatabase(couchDbMembershipTable);
        }

        public Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            return DoAndLog(nameof(InsertRow), () =>
            {
                return UpsertRow(clusterId, entry, null, tableVersion);
            });
        }

        public Task<MembershipTableData> ReadAll()
        {
            return DoAndLog(nameof(ReadAll), () =>
            {
                return ReadAll(clusterId);
            });
        }

        public Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            return DoAndLog(nameof(ReadRow), () =>
            {
                return ReadRow(clusterId, key);
            });
        }

        public Task UpdateIAmAlive(MembershipEntry entry)
        {

            return DoAndLog(nameof(UpdateRow), () =>
            {
                return UpdateIAmAlive(clusterId,
                    entry.SiloAddress,
                    entry.IAmAliveTime);
            });
        }

        public Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            return DoAndLog(nameof(UpdateRow), () =>
            {
                return UpsertRow(clusterId, entry, etag, tableVersion);
            });
        }

        private async Task<MembershipTableData> ReadRow(string deploymentId, SiloAddress address)
        {
           
            var (etag, deployment) = await GetDeployment(deploymentId);
            
            if (deployment == null)
            {
                return new MembershipTableData(NotFound);
            }

            return deployment.ToTable(BuildKey(address), etag);
        }

        private static string BuildKey(SiloAddress address)
        {
            return address.ToParsableString().Replace('.', '_');
        }

        private async Task<bool> UpsertRow(string deploymentId, MembershipEntry entry, string? etag, TableVersion tableVersion)

        {
            try
            {
                var subDocument = MembershipBase.Create<DeploymentMembership>(entry);

                var memberKey = $"Members.{BuildKey(entry.SiloAddress)}";

                var response = await couchdb.Get<DeploymentDocument>(couchDbMembershipTable, deploymentId, null);

                var deployDoc = response?.Doc;

                
                if(deployDoc == null)
                {
                    deployDoc = new DeploymentDocument()
                    {
                        Id = deploymentId,
                        Version = tableVersion.Version,
                        Members = new Dictionary<string, DeploymentMembership>()
                        {
                            {memberKey, subDocument }
                        }

                    };
                    await couchdb.Put(couchDbMembershipTable, deploymentId, deployDoc, response?.Rev);
                    return true;
                }
                if (deployDoc.Version == tableVersion.Version)
                    return true;

                if (deployDoc.Members.ContainsKey(memberKey))
                {
                    deployDoc.Members[memberKey] = subDocument;
                }
                else
                {
                    deployDoc.Members.TryAdd(memberKey, subDocument);
                }
                
                deployDoc.Version = tableVersion.Version;

                await couchdb.Put(couchDbMembershipTable, deploymentId, deployDoc, response?.Rev);
                return true;
            }
            catch (CouchDbException ex)
            {
                if (ex.Message == "Document update conflict.")
                    return false;

                throw;
            }
        }

        private async Task<T> DoAndLog<T>(string actionName, Func<Task<T>> action)
        {
            logger.LogDebug($"{nameof(CouchDbMembershipTable)}.{actionName} called.");

            try
            {
                return await action();
            }
            catch (Exception ex)
            {

                logger.LogError((int)CouchDbProviderErrorCode.MembershipTable_Operations, ex, $"{nameof(CouchDbMembershipTable)}.{actionName} failed. Exception={ex.Message}");

                throw;
            }
        }

        private Task DoAndLog(string actionName, Func<Task> action)
        {
            return DoAndLog(actionName, async () =>
            {
                await action();

                return true;
            });
        }


        private async Task<MembershipTableData> ReadAll(string deploymentId)
        {
            var (etag, deployment) = await GetDeployment(deploymentId);

            if (deployment == null)
            {
                return new MembershipTableData(NotFound);
            }

            return deployment.ToTable(etag);
        }


        private async Task<(string?, DeploymentDocument?)> GetDeployment(string deploymentId)
        {
            var response = await couchdb.Get<DeploymentDocument>(couchDbMembershipTable, deploymentId, null);
            return (response?.Rev, response?.Doc);
        }

        private async Task UpdateIAmAlive(string deploymentId, SiloAddress address, DateTime iAmAliveTime)
        {
            var key = BuildKey(address);
            var (etag, deployment) = await GetDeployment(deploymentId);
            if (deployment == null) return;

            if (!deployment.Members.ContainsKey(key))
                return;

           
            deployment.Members[key].IAmAliveTime = LogFormatter.PrintDate(iAmAliveTime);

            await couchdb.Put(couchDbMembershipTable, deploymentId, deployment, etag);
           
        }

        private async Task CleanupDefunctSiloEntries(string deploymentId, DateTimeOffset beforeDate)
        {
            var (etag, deployment) = await GetDeployment(deploymentId);

            if (deployment == null) return;

            var keys = deployment.Members.Keys;

            foreach (var key in keys)
            {
                var member = deployment.Members[key];
                if (member.Status != (int)SiloStatus.Active && member.Timestamp < beforeDate)
                {
                    deployment.Members.Remove(key);
                }
            }

            await couchdb.Put(couchDbMembershipTable, deploymentId, deployment, etag);
          
            
        }

       
    }
}

