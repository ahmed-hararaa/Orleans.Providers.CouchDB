using System;
using Orleans.Runtime;
using System.Net;
using SiloAddressClass = Orleans.Runtime.SiloAddress;

namespace Orleans.Providers.CouchDB.Membership
{
    public class MembershipBase
    {
        
        public string? ETag { get; set; }

        
        public string? HostName { get; set; }

        
        public string? SiloAddress { get; set; }

        
        public string? SiloName { get; set; }

        
        public string? RoleName { get; set; }

       
        public string? StatusText { get; set; }

        
        public string? IAmAliveTime { get; set; }

        
        public string? StartTime { get; set; }

        
        public int ProxyPort { get; set; }

        
        public int UpdateZone { get; set; }

        
        public int FaultZone { get; set; }

        
        public int Status { get; set; }


        public List<CouchDbSuspectTime> SuspectTimes { get; set; } = new List<CouchDbSuspectTime>();

        
        public DateTime Timestamp { get; set; }

        public static T Create<T>(MembershipEntry entry) where T : MembershipBase, new()
        {
            var suspectTimes =
                entry.SuspectTimes?.Select(CouchDbSuspectTime.Create).ToList() ?? new List<CouchDbSuspectTime>();

            return new T
            {
                
                FaultZone = entry.FaultZone,
                ETag = Guid.NewGuid().ToString(),
                HostName = entry.HostName,
                IAmAliveTime = LogFormatter.PrintDate(entry.IAmAliveTime),
                ProxyPort = entry.ProxyPort,
                RoleName = entry.RoleName,
                SiloAddress = entry.SiloAddress.ToParsableString(),
                SiloName = entry.SiloName,
                Status = (int)entry.Status,
                StatusText = entry.Status.ToString(),
                StartTime = LogFormatter.PrintDate(entry.StartTime),
                SuspectTimes = suspectTimes,
                Timestamp = entry.IAmAliveTime,
                UpdateZone = entry.UpdateZone
            };
        }

        public MembershipEntry ToEntry()
        {
            return new MembershipEntry
            {
                FaultZone = FaultZone,
                HostName = HostName,
                IAmAliveTime = LogFormatter.ParseDate(IAmAliveTime),
                ProxyPort = ProxyPort,
                RoleName = RoleName,
                SiloAddress = SiloAddress == null ? null :SiloAddressClass.FromParsableString(SiloAddress),
                SiloName = SiloName,
                Status = (SiloStatus)Status,
                StartTime = LogFormatter.ParseDate(StartTime),
                SuspectTimes = SuspectTimes.Select(x => x.ToTuple()).ToList(),
                UpdateZone = UpdateZone
            };
        }

        public Uri ToGatewayUri()
        {
            var siloAddress = SiloAddressClass.FromParsableString(SiloAddress);

            return SiloAddressClass.New(new IPEndPoint(siloAddress.Endpoint.Address, ProxyPort), siloAddress.Generation).ToGatewayUri();
        }
    }
}

