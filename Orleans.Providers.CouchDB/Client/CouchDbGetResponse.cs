using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client
{
    public class CouchDbGetResponse<T>
    {

        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("_rev")]
        public string? Rev { get; set; }

        public T? Doc { get; set; }
    }
}

