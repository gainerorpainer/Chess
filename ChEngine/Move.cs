namespace ChEngine
{
    public enum MoveType
    {
        Move = 0,
        Take,
        CastleKingside,
        CastleQueenside,
        PromoteKnight = 'N',
        PromoteBishop = 'B',
        PromoteRook = 'R',
        PromoteQueen = 'Q',
    }

    public struct Move
    {
        public int From;
        public int To;
        public MoveType MoveType;

        public Move(int from, int to, MoveType moveType)
        {
            From = from;
            To = to;
            MoveType = moveType;
        }
    }
}