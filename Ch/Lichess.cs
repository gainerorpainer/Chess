using Ch.LichessTypes;
using Newtonsoft.Json;
using System;
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
    static class Lichess
    {
        const string TOKEN = "02RchmSYpcJt87OJ";
        private const string EVENTSTREAM_URI = "https://lichess.org/api/stream/event";

        readonly static TimeSpan TIMEOUT = new TimeSpan(0, 0, 1);
        private static readonly HttpClient client = new HttpClient();


        static Lichess()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);
        }

        private static string HttpGet(string uri)
        {
            var task = client.GetStringAsync(uri);

            // wait up till one second
            if (!task.Wait(1000))
                return null;

            return task.Result;

        }

        internal static List<string> GetGames()
        {
            return HttpGetLines<Event>(EVENTSTREAM_URI).Where(x => x.type == "gameStart").Select(x => x.game.id).ToList();
        }

        internal static void Accept(string gameId)
        {
            HttpPost($"https://lichess.org/api/challenge/{gameId}/accept", new StringContent(""));
        }

        private static T HttpGet<T>(string uri)
        {
            return JsonConvert.DeserializeObject<T>(HttpGet(uri));
        }

        private static List<T> HttpGetLines<T>(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + TOKEN);
            request.Timeout = (int)TIMEOUT.TotalMilliseconds;

            var responseStream = request.GetResponse().GetResponseStream();

            StreamReader reader = new StreamReader(responseStream);

            var result = new List<T>();
            string line = reader.ReadLine();
            while (line?.Length > 0)
            {
                result.Add(JsonConvert.DeserializeObject<T>(line));
                line = reader.ReadLine();
            }

            return result;
        }

        private static void HttpPost(string uri, HttpContent content)
        {
            var result = client.PostAsync(uri, content).Result;

            if (!result.IsSuccessStatusCode)
                throw new WebException("NOT OK: " + result.ReasonPhrase + " " + result.Content.ReadAsStringAsync().Result);
        }

        private static void HttpPost(string uri) => HttpPost(uri, new StringContent(""));

        public static string GetUsername()
        {
            return HttpGet<Account>("https://lichess.org/api/account").username;
        }

        public static List<string> GetChallenges()
        {
            return HttpGetLines<Event>(EVENTSTREAM_URI)
                .Where(x => x.type == "challenge").Select(x => x.challenge.id).ToList();
        }

        public static void Chat(string gameId, string message)
        {
            // prepare
            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                {  "room", "player"},
                { "text", message}
            });
            HttpPost($"https://lichess.org/api/bot/game/{gameId}/chat", content);
        }

        public static void Resign(string runningGame)
        {
            HttpPost($"https://lichess.org/api/bot/game/{runningGame}/resign");
        }
    }
}
