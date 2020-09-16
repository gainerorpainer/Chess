using ChEngine;
using System;
using System.Linq;

namespace IntegrationTestCh
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine e = new Engine(isWhite: true);

            // let the engine crunch on this
            e.ReactToMove(Enumerable.Empty<Move>());
        }
    }
}
