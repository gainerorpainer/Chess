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
    }
}