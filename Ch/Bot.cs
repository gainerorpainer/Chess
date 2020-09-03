using Ch.LichessTypes;
using ChEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ch
{
    class Bot
    {
        public Action<string> MoveAction { get; set; }
        public Engine Engine { get; set; }

        public Bot(GameStartEvent gamestartevent, Action<string> moveAction)
        {
            MoveAction = moveAction;

            // check who is first
            Engine = new Engine("");
            if (gamestartevent.white.id == "lorenzobot")
            {
                MoveAction(Engine.ReactToMove(""));
            }
        }

        public void MakeMove(string moves)
        {
            MoveAction(Engine.ReactToMove(moves));
        }
    }
}
