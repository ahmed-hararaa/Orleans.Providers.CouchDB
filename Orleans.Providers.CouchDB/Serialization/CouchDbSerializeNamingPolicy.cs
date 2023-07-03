using System;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Serialization
{
    public class CouchDbDeserializeNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (name == "Id") return "_id";
            if (name == "Rev") return "_rev";
            return JsonNamingPolicy.CamelCase.ConvertName(name);
        }
    }
}

