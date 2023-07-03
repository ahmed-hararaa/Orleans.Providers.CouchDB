using System;
namespace Orleans.Providers.CouchDB.Membership
{
    public sealed class DeploymentDocument
    {
       
        public string? Id { get; set; }

        
        public int Version { get; set; }

      
       // public string? Rev { get; set; }


        public Dictionary<string, DeploymentMembership> Members { get; set; } = new Dictionary<string, DeploymentMembership>();

        public TableVersion ToTableVersion(string? etag)
        {
            return new TableVersion(Version, etag);
        }

        public MembershipTableData ToTable(string? etag)
        {
            return new MembershipTableData(Members.Values.Select(x => Tuple.Create(x.ToEntry(), x.ETag)).ToList(), ToTableVersion(etag));
        }

        public MembershipTableData ToTable(string address, string? etag)
        {
            return new MembershipTableData(Members.Where(x => x.Key == address).Select(x => Tuple.Create(x.Value.ToEntry(), x.Value.ETag)).ToList(), ToTableVersion(etag));
        }
    }
}

