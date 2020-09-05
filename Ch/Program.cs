using Ch.LichessTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Ch
{
    class Program
    {


        static void Main(string[] args)
        {
            // start a thread for listening for events
            Lichess lichess = new Lichess();

            Console.WriteLine("Setting up connection...");
            Console.WriteLine($"Logged in as \"{lichess.GetUsername()}\"");

            string gameId;

            Console.WriteLine("Checking running games...");
            List<string> runningGames = lichess.GetGameIds();
            if (runningGames.Count == 0)
            {
                Console.WriteLine("Waiting you to invite me to a game..");

                string challengeId = WaitForFirst(() => lichess.GetChallenges());
                Console.WriteLine($"Taking challenge: \"{challengeId}\"");

                lichess.Accept(challengeId);

                gameId = WaitForFirst(() => lichess.GetGameIds());
            }
            else
            {
                gameId = runningGames.First();

                foreach (var runningGame in runningGames.Skip(1))
                {
                    lichess.Resign(runningGame);
                    Console.WriteLine($"Resigned \"{runningGame}\"");
                }
            }

            Console.WriteLine($"Taking game: \"{gameId}\"");
            lichess.BeginGameListen(gameId);

            GameStartEvent gameStart = new GameStartEvent(WaitForFirst(() => lichess.GetGameEvents()));

            lichess.Chat(gameId, "Hello from LorenzoBot");

            // load engine
            Bot bot = new Bot(gameStart, (move) => lichess.Move(gameId, move));

            // until game end, see if i need to move
            while (true)
            {
                GameEvent ev = WaitForFirst(() => lichess.GetGameEvents());

                if (ev.type == "gameState" && ev.status == "resign")
                    break;

                if (ev.type == "gameState")
                    bot.OnNewMove(ev.moves);
            }
        }

        private static T WaitForFirst<T>(Func<List<T>> source)
        {
            do
            {
                var items = source();
                if (items.Count > 0)
                    return items.First();

                // Animation
                Console.Write("/");
                Thread.Sleep(250);

                ClearCurrentConsoleLine();
                Console.Write("\\");
                Thread.Sleep(250);

                ClearCurrentConsoleLine();
            } while (true);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
