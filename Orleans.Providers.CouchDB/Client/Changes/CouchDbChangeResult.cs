using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client.Changes
{
    public class CouchDbChangeResult
    {

        [JsonPropertyName("seq")]
        public string? Sequence { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("changes")]
        public List<CouchDbDocumentLeave> Changes { get; set; } = new List<CouchDbDocumentLeave>();
    }

    public class CouchDbChangeResult<T>
    {

        [JsonPropertyName("seq")]
        public string? Sequence { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("doc")]
        public T? Document { get; set; }

        [JsonPropertyName("changes")]
        public List<CouchDbDocumentLeave> Changes { get; set; } = new List<CouchDbDocumentLeave>();
    }
}

