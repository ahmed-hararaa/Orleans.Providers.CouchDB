using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orleans.Providers.CouchDB.Client.Changes
{
    public class CouchDbChangesResponse
    {
       
        public CouchDbResponseType Type { get; }

        public string? Payload { get; }

        public CouchDbChangesPayload? Parse()
        {
            if (Type == CouchDbResponseType.Heartbeat || Payload == null) return null;
            return JsonSerializer.Deserialize<CouchDbChangesPayload>(Payload);

        }

        public CouchDbChangesPayload<T>? Parse<T>()
        {
            if (Type == CouchDbResponseType.Heartbeat || Payload == null) return null;
            return JsonSerializer.Deserialize<CouchDbChangesPayload<T>>(Payload);
        }

        public CouchDbChangesResponse(CouchDbResponseType type, string? payload)
        {
            Type = type;
            Payload = payload;
        }

    }
}

