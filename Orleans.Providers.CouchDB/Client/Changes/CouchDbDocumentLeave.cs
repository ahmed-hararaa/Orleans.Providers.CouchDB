using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client.Changes
{
    public class CouchDbDocumentLeave
    {
        [JsonPropertyName("rev")]
        public string? Rev { get; set; }
    }
}

