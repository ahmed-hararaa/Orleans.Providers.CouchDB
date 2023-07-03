using System;
using System.Text.Json.Nodes;

namespace Orleans.Providers.CouchDB.Serialization
{
	public interface ICouchDbDocumentSerializer
    {
        string Serialize<T>(T doc);

        /// <summary>
        /// Deserializes the provided value.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        /// <typeparam name="T">The output type.</typeparam>
        /// <returns>The deserialized state.</returns>
        T? Deserialize<T>(string value);
    }
}

