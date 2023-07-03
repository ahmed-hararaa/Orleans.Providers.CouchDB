using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client.Changes
{
	public class CouchDbChangesPayload
	{
        [JsonPropertyName("last_seq")]
        public string? LastSequence { get; set; }

        [JsonPropertyName("pending")]
        public int? Pending { get; set; }

        [JsonPropertyName("results")]
        public List<CouchDbChangeResult> Results { get; set; } = new List<CouchDbChangeResult>();
    }

    public class CouchDbChangesPayload<T>
    {
        [JsonPropertyName("last_seq")]
        public string? LastSequence { get; set; }

        [JsonPropertyName("pending")]
        public int? Pending { get; set; }

        [JsonPropertyName("results")]
        public List<CouchDbChangeResult<T>> Results { get; set; } = new List<CouchDbChangeResult<T>>();
    }
}

