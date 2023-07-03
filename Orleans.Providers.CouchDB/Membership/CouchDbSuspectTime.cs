using System;
using Orleans.Runtime;

namespace Orleans.Providers.CouchDB.Membership
{
	public class CouchDbSuspectTime
	{
       
        public string? Address { get; set; }

       
        public string? IAmAliveTime { get; set; }


        public static CouchDbSuspectTime Create(Tuple<SiloAddress, DateTime> tuple)
        {
            return new CouchDbSuspectTime { Address = tuple.Item1.ToParsableString(), IAmAliveTime = LogFormatter.PrintDate(tuple.Item2) };
        }

        public Tuple<SiloAddress, DateTime> ToTuple()
        {
            return Tuple.Create(SiloAddress.FromParsableString(Address), LogFormatter.ParseDate(IAmAliveTime));
        }
    }
}

