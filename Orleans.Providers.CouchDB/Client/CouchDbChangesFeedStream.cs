using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Orleans.Providers.CouchDB.Client.Auth;
using Orleans.Providers.CouchDB.Client.Changes;
namespace Orleans.Providers.CouchDB.Client
{
	public class CouchDbChangesFeedStream
	{
        private readonly string endpoint;
        private readonly string database;
        private readonly IHttpClientFactory factory;
        private readonly ICouchDbAuth auth;
        private readonly CouchDbChangesOptions options;

        public CouchDbChangesFeedStream(
            string endpoint,
            string database,
            IHttpClientFactory factory,
            ICouchDbAuth auth,
            CouchDbChangesOptions options)
		{
            this.endpoint = endpoint;
            this.database = database;
            this.factory = factory;
            this.auth = auth;
            this.options = options;
        }

        public async IAsyncEnumerable<CouchDbChangesResponse> Changes([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) //main loop
            {
                Console.WriteLine("Starting new connection");
                using var stream = await GetStream(cancellationToken);
                CouchDbChangesResponse? response = null;
                while (true)
                {
                    try
                    {
                        response = await GetResult(stream, cancellationToken);

                    }
                   
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                        yield break;
                    }

                    if (response != null)
                        yield return response;

                    if (options.Feed == CouchDbChangesFeed.Normal || options.Feed == CouchDbChangesFeed.Longpoll)
                        yield break;
                }

            }

            Console.WriteLine("Stream closed");

        }

        async Task<StreamReader> GetStream(CancellationToken token)
        {
            var httpClient = await auth.Authenticate(endpoint, factory);
            if (options.Feed == CouchDbChangesFeed.EventSource)
                httpClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");

            httpClient.Timeout = TimeSpan.FromDays(1);
            var query = options.ToQueryParameters();
            Console.WriteLine(query);
            return new StreamReader(
               await httpClient.GetStreamAsync($"{endpoint}/{database}/_changes?{query}", token));
        }

        Task<CouchDbChangesResponse?> GetResult(StreamReader stream, CancellationToken token)
        {
            var feed = options.Feed;
            if (feed == CouchDbChangesFeed.Continuous || feed == CouchDbChangesFeed.EventSource)
            {
                return ReadSingle(stream, token);
            }

            return ReadAll(stream, token);
        }

        async Task<CouchDbChangesResponse?> ReadAll(StreamReader stream, CancellationToken token)
        {
            var all = await stream.ReadToEndAsync(token);
            if (string.IsNullOrEmpty(all))
            {
                return null;
            }

            return new CouchDbChangesResponse(CouchDbResponseType.Changes, all); 
            
        }

        async Task<CouchDbChangesResponse?> ReadSingle(StreamReader stream, CancellationToken token)
        {
            if (options.Feed == CouchDbChangesFeed.Continuous)
            {
                var line = await ReadLine(stream, token);
                if (line == null)
                    return null;
                if (string.IsNullOrEmpty(line))
                    return new CouchDbChangesResponse(CouchDbResponseType.Heartbeat, null);

                return new CouchDbChangesResponse(CouchDbResponseType.Changes, line);
                
            }
            return await ReadEventSource(stream, token);
        }

        async Task<CouchDbChangesResponse?> ReadEventSource(StreamReader stream, CancellationToken token)
        {
            var payload = new EventSourcePayload();
            while (true) //read payload loop
            {
                var line = await ReadLine(stream, token);
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var parts = line.Split(":", count: 2);
                if (parts.Length != 2) break;

                var property = parts[0];
                var value = parts[1];

                if (property.ToLowerInvariant() == "id")
                {
                    payload.Id = value;
                }
                else if (property.ToLowerInvariant() == "data")
                {
                    payload.Data = value;
                }
                else if (property.ToLowerInvariant() == "event")
                {
                    payload.Event = value;
                }
            }

            if (payload.Event == "heartbeat" || payload.Id == null || payload.Data == null)
            {
                Console.WriteLine("Heartbeat");
                return new CouchDbChangesResponse(CouchDbResponseType.Heartbeat, null);
            }
            //var result = JsonSerializer.Deserialize<CouchDbChangeResult>(payload.Data);
            //if (result == null) return null;
            return new CouchDbChangesResponse(CouchDbResponseType.Changes, payload.Data);
        }

        async Task<string?> ReadLine(StreamReader stream, CancellationToken token)
        {
            var timeout = options.Heartbeat.Add(TimeSpan.FromSeconds(10));
            var readLineTask = stream.ReadLineAsync(token).AsTask();

            var timeoutTask = Task.Delay(timeout);
            var winner = await Task.WhenAny(readLineTask, timeoutTask);
            if (winner == timeoutTask)
            {
                throw new TimeoutException();
            }
            return await readLineTask;
        }

        class EventSourcePayload
        {
            public string? Id { get; set; }

            public string? Data { get; set; }

            public string? Event { get; set; }
        }
    }
}

