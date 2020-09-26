using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    struct ColRowCoord
    {
        public int Col { get; set; }
        public int Row { get; set; }

        public int Index => Col + 8 * Row;

        public ColRowCoord(int col, int row)
        {
            Col = col;
            Row = row;
        }
    }

    struct IndexCoord
    {
        public int Index { get; set; }

        public IndexCoord(int index)
        {
            Index = index;
        }
    }
}
