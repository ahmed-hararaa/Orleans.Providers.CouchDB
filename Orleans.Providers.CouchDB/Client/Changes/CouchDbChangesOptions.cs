using System;
using System.Web;

namespace Orleans.Providers.CouchDB.Client.Changes
{
    public class CouchDbChangesOptions
    {
        public string Since { get; set; } = "0";

        public TimeSpan Heartbeat { get; set; } = TimeSpan.FromSeconds(60);

        public bool Descending { get; set; }

        public bool Conflicts { get; set; }

        public string? Filter { get; set; }

        public CouchDbChangesFeed Feed { get; set; } = CouchDbChangesFeed.Normal;

        public int? Limit { get; set; }

        public bool IncludeDocs { get; set; }

        public string FeedStr
        {
            get
            {
                string feed;
                switch (Feed)
                {
                    case CouchDbChangesFeed.Longpoll:
                        feed = "longpoll";
                        break;
                    case CouchDbChangesFeed.Continuous:
                        feed = "continuous";
                        break;
                    case CouchDbChangesFeed.EventSource:
                        feed = "eventsource";
                        break;
                    default:
                        feed = "normal";
                        break;
                }

                return feed;
            }
        }

        public string ToQueryParameters()
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["since"] = Since;
            query["heartbeat"] = Heartbeat.TotalMilliseconds.ToString();
            query["descending"] = Descending.ToString().ToLowerInvariant();
            query["conflicts"] = Conflicts.ToString().ToLowerInvariant();
            query["include_docs"] = IncludeDocs.ToString().ToLowerInvariant();
            query["feed"] = FeedStr;
            if (Limit != null)
            {
                query["limit"] = Limit.ToString();
            }
            if (Filter != null)
            {
                query["filter"] = Filter;
            }
            return (query.ToString() ?? "");
        }
    }
}

