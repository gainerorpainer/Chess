using Ch.LichessTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ch
{
    class Bot
    {
        public Action<string> MoveAction { get; set; }

        public Bot(GameStartEvent gamestartevent, Action<string> moveAction)
        {
            MoveAction = moveAction;

            // check who is first
            if (gamestartevent.white.id == "lorenzobot")
                MakeMove("");
            else
                ;
        }

        public void MakeMove(string moves)
        {
            MoveAction("e2e4");
        }
    }
}
