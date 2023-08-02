using System;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Storage
{
	public sealed class DocWrapper<T>
		
	{
		[JsonPropertyName("_id")]
		public string? Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("_rev")]
        public string? ETag { get; set; }

		[JsonPropertyName("$state")]
		public T? State { get; set; }
	}
}

