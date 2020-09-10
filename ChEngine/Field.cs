using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    public struct Field
    {
        public bool IsWhite;
        public TypeOfFigure Figure;

        public Field(bool isWhite, TypeOfFigure type)
        {
            IsWhite = isWhite;
            Figure = type;
        }
    }
}
