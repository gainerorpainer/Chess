using Ch.LichessTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Ch
{
    class Lichess
    {
        const string TOKEN = "02RchmSYpcJt87OJ";

        readonly static TimeSpan TIMEOUT = new TimeSpan(0, 0, 1);


        public ConcurrentQueue<Event> Events { get; private set; } = new ConcurrentQueue<Event>();
        public ConcurrentQueue<GameEvent> GameEvents { get; private set; } = new ConcurrentQueue<GameEvent>();

        private readonly CancellationTokenSource cancellationTokenSource_ = new CancellationTokenSource();
        private Thread eventThread_;
        private Thread gameThread_;

        public Lichess()
        {
            StartEventThread();
        }

        ~Lichess()
        {
            CancelSidethreads();
        }

        private void CancelSidethreads()
        {
            cancellationTokenSource_.Cancel();

            eventThread_?.Join();
            gameThread_?.Join();
        }

        private void StartEventThread()
        {
            eventThread_ = new Thread(() => HttpStream("https://lichess.org/api/stream/event", Events));
            eventThread_.Start();
        }

        public void BeginGameListen(string gameId)
        {
            if (gameThread_ != null)
                EndGameListen();

            gameThread_ = new Thread(() => HttpStream($"https://lichess.org/api/bot/game/stream/{gameId}", GameEvents));
            gameThread_.Start();
        }

        public void EndGameListen()
        {
            CancelSidethreads();

            gameThread_ = null;

            // Restart event thread
            StartEventThread();
        }


        private string HttpGet(string uri)
        {
            HttpClient client = GetClient();

            var task = client.GetStringAsync(uri);

            // wait up till one second
            if (!task.Wait(1000))
                return null;

            return task.Result;
        }

        private T HttpGet<T>(string uri) => JsonConvert.DeserializeObject<T>(HttpGet(uri));

        private void HttpPost(string uri, HttpContent content)
        {
            HttpClient client = GetClient();

            var result = client.PostAsync(uri, content).Result;

            if (!result.IsSuccessStatusCode)
                throw new WebException("NOT OK: " + result.ReasonPhrase + " " + result.Content.ReadAsStringAsync().Result);
        }

        private void HttpPost(string uri) => HttpPost(uri, new StringContent(""));

        private void HttpStream<T>(string uri, ConcurrentQueue<T> target)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + TOKEN);
            request.Timeout = (int)TIMEOUT.TotalMilliseconds;

            WebResponse webResponse = request.GetResponse();
            var responseStream = webResponse.GetResponseStream();

            StreamReader reader = new StreamReader(responseStream);

            string line;
            while (!reader.EndOfStream && !cancellationTokenSource_.Token.IsCancellationRequested)
            {
                line = reader.ReadLine();

                if (line?.Length > 0)
                    target.Enqueue(JsonConvert.DeserializeObject<T>(line));
            }
        }

        private static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);
            return client;
        }

        private IEnumerable<Event> ConsumeEvents()
        {
            while (Events.TryDequeue(out Event ev))
                yield return ev;
        }

        private IEnumerable<GameEvent> ConsumeGameEvents()
        {
            while (GameEvents.TryDequeue(out GameEvent ev))
                yield return ev;
        }

        public List<GameEvent> GetGameEvents() => ConsumeGameEvents().ToList();

        public List<string> GetGameIds() => ConsumeEvents().Where(x => x.type == "gameStart").Select(x => x.game.id).ToList();

        public List<string> GetChallenges() => ConsumeEvents().Where(x => x.type == "challenge").Select(x => x.challenge.id).ToList();

        public string GetUsername() => HttpGet<Account>("https://lichess.org/api/account").username;

        public void Chat(string gameId, string message) =>
            // prepare
            HttpPost($"https://lichess.org/api/bot/game/{gameId}/chat", new FormUrlEncodedContent(new Dictionary<string, string>{
                { "room", "player"},
                { "text", message}
            }));

        public void Accept(string challengeId) => HttpPost($"https://lichess.org/api/challenge/{challengeId}/accept", new StringContent(""));
        public void Resign(string runningGame) => HttpPost($"https://lichess.org/api/bot/game/{runningGame}/resign");

        public void Move(string gameId, string move)
        {
            HttpPost($"https://lichess.org/api/bot/game/{gameId}/move/{move}");
        }
    }
}
