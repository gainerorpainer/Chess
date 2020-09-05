using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    public struct Field
    {
        public bool IsWhite;
        public FigureType Figure;

        public Field(bool isWhite, FigureType type)
        {
            IsWhite = isWhite;
            Figure = type;
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
