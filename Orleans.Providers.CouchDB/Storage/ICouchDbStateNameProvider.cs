using System;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Storage
{
	public interface ICouchDbStateNameProvider
	{
		string GetName(string stateName);
	}

    public class CouchDbDefaultStateNameProvider : ICouchDbStateNameProvider
    {
        
        public string GetName(string stateName)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(stateName);
        }
    }
}

