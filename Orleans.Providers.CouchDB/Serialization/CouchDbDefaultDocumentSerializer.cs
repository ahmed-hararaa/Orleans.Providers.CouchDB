using System;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Serialization
{
    public class CouchDbDefaultDocumentSerializer : ICouchDbDocumentSerializer
    {
        public T? Deserialize<T>(string value)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new CouchDbDeserializeNamingPolicy(),
            };

            return JsonSerializer.Deserialize<T>(value, options: serializeOptions);
        }

        public string Serialize<T>(T doc)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new CouchDbSerlizeNamingPolicy()
            };

            return JsonSerializer.Serialize(doc, options: serializeOptions);
        }
    }
}

