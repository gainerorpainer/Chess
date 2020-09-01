using System;
using System.Collections.Generic;
using System.Text;

namespace Ch.LichessTypes
{
    class GameEvent
    {
        public string type { get; set; }
        public Player white { get; set; }
        public Player black { get; set; }
        public string moves { get; set; }
        public string status { get; set; }
    }

    class GameStartEvent
    {
        public Player white { get; private set; }
        public Player black { get; private set; }

        public GameStartEvent(GameEvent ev)
        {
            if (ev.type != "gameFull")
                throw new LichessConversionException("Invalid conversion");

            white = ev.white;
            black = ev.black;
        }
    }
}
