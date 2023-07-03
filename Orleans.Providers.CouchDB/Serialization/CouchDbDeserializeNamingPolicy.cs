using System;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Serialization
{
    public class CouchDbSerlizeNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (name == "Id" || name == "id") return "_id";
            if (name == "Rev") return "_rev";
            return JsonNamingPolicy.CamelCase.ConvertName(name);
        }
           
    }
}

