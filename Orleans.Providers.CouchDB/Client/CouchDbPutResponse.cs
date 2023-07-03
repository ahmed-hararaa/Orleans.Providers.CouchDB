using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client
{
	public class CouchDbPutResponse
	{
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

		[JsonPropertyName("id")]
		public string? Id { get; set; }

        [JsonPropertyName("rev")]
        public string? Rev { get; set; }
	}
}

