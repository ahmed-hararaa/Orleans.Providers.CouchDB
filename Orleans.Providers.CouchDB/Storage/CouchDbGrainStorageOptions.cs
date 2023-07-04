using System;
namespace Orleans.Providers.CouchDB.Storage
{
	public class CouchDbGrainStorageOptions : CouchDbOptions
	{
		public ICouchDbStateNameProvider? NameProvider { get; set; }
	}
}

