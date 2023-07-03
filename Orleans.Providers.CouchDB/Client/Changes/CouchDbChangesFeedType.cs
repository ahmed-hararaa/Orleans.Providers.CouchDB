using System;
namespace Orleans.Providers.CouchDB.Client.Changes
{
    public enum CouchDbChangesFeed
    {
        Normal,
        Longpoll,
        Continuous,
        EventSource
    }
}

