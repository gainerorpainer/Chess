using Ch.LichessTypes;
using ChEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ch
{
    class Bot
    {
        public Action<Move> MoveAction { get; set; }
        public Engine Engine { get; set; }
        public bool IsWhite { get; set; }
        public int MovesDone { get; set; } = 1;

        public Bot(GameStartEvent gamestartevent, Action<Move> moveAction)
        {
            MoveAction = moveAction;

            // check who is first
            IsWhite = gamestartevent.white.id == "lorenzobot";
            Engine = new Engine(IsWhite);

            OnNewMove(gamestartevent.state.moves);
        }

        private static bool GetWhiteToMove(string moves) => moves.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length % 2 == 0;

        /// <summary>
        /// Triggers the engine to consider
        /// You may call this function even if there are no new moves!
        /// </summary>
        /// <param name="moves">Moves space separated</param>
        public void OnNewMove(string moves)
        {
            if (IsWhite == GetWhiteToMove(moves))
            {
                IEnumerable<Move> movesParsed = moves.Split(' ').Select(x => UCINotation.ParseMove(x));
                Move bestMove = Engine.ReactToMove(movesParsed);
                MoveAction(bestMove);
            }

        }
    }
}
