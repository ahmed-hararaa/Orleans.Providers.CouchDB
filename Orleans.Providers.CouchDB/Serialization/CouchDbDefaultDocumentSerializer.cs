using System;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Serialization
{
    public class CouchDbDefaultDocumentSerializer : ICouchDbDocumentSerializer
    {
        public T? Deserialize<T>(string value)
        {
            
            return JsonSerializer.Deserialize<T>(value);
        }

        public string Serialize<T>(T doc)
        {
            return JsonSerializer.Serialize(doc);
        }
    }
}

