using System;

namespace ChEngine
{
    public struct PlayerOption
    {
        public byte EnPassantOptions;
        public bool KingsideCastle;
        public bool QueensideCastle;

        public static readonly PlayerOption DefautOption = new PlayerOption() { EnPassantOptions = 0, KingsideCastle = true, QueensideCastle = true };


        // set bit number colNumber
        public void GiveEnpassantOnCol(int colNumber) => EnPassantOptions = (byte)(EnPassantOptions | (1 << colNumber));

        // Clear all bits
        public void ClearEnpassant() => EnPassantOptions = 0;

        // test bit
        public bool CheckEnpassantOnCol(int colNumber) => (EnPassantOptions & (1 << colNumber)) != 0;

        public override bool Equals(object obj)
        {
            if (obj is PlayerOption p)
            {
                return
                    (p.EnPassantOptions == EnPassantOptions)
                    &&
                    (p.KingsideCastle == KingsideCastle)
                    &&
                    (p.QueensideCastle == QueensideCastle)
                    ;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EnPassantOptions, KingsideCastle, QueensideCastle);
        }

        public static bool operator ==(PlayerOption left, PlayerOption right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerOption left, PlayerOption right)
        {
            return !(left == right);
        }
    }
}