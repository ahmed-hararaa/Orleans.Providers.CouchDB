using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client
{
	public class CouchDbError
	{
		[JsonPropertyName("error")]
		public string? Error { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
	}
}

