using System.Runtime.Serialization;
using System.Text.Json;

namespace Orleans.Providers.CouchDB.Client
{
    [Serializable]
    internal class CouchDbException : Exception
    {
        public CouchDbException()
        {
        }

        public CouchDbException(string? message) : base(message)
        {
        }

        public CouchDbException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CouchDbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static CouchDbException FromBody(string body)
        {
            var error = JsonSerializer.Deserialize<CouchDbError>(body);
            if (error != null)
                return new CouchDbException(message: error.Reason);
            return new CouchDbException(message: "Unknown error");
        }
    }
}