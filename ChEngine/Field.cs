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

        public override bool Equals(object obj)
        {
            if (obj is Field f)
            {
                // Special case: If empty, color does not matter
                if (f.Figure == TypeOfFigure.EMPTY)
                    return Figure == TypeOfFigure.EMPTY;

                return
                    (f.IsWhite == IsWhite)
                    &&
                    (f.Figure == Figure)
                    ;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsWhite, Figure);
        }

        public static bool operator ==(Field left, Field right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Field left, Field right)
        {
            return !(left == right);
        }
    }
}
