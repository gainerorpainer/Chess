using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Ch
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Setting up connection...");
            Console.WriteLine($"Logged in as \"{Lichess.GetUsername()}\"");

            Console.WriteLine("Checking running games...");
            foreach (var runningGame in Lichess.GetGames())
            {
                Lichess.Resign(runningGame);
                Console.WriteLine($"Resigned \"{runningGame}\"");
            }

            Console.WriteLine("Waiting you to invite me to a game..");

            string challengeId;

            do
            {
                var challenges = Lichess.GetChallenges();

                if (challenges.Count > 0)
                {
                    challengeId = challenges.First();
                    Console.WriteLine($"Taking challenge: \"{challengeId}\"");
                    break;
                }

                Console.WriteLine("No challenges yet...");
                Thread.Sleep(250);
            } while (true);

            Lichess.Accept(challengeId);

            string gameId;

            do
            {
                var games = Lichess.GetGames();

                if (games.Count > 0)
                {
                    gameId = games.First();
                    Console.WriteLine($"Taking game: \"{gameId}\"");
                    break;
                }

                Console.WriteLine("No challenges yet...");
                Thread.Sleep(250);
            } while (true);

            Lichess.Chat(gameId, "Hello from LorenzoBot");
        }
    }
}
