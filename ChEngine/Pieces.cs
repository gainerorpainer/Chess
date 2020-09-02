using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    public struct Figure
    {
        public bool IsWhite;
        public FigureType Type;

        public Figure(bool isWhite, FigureType type)
        {
            IsWhite = isWhite;
            Type = type;
        }
    }

    public enum FigureType
    {
        EMPTY,
        Rook,
        Knight,
        Bishop,
        Queen,
        King,
        Pawn
    }
}
