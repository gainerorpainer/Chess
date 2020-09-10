namespace ChEngine
{
    public enum TypeOfMove
    {
        Move = 'M',
        Take = 'T',
        CastleKingside = '1',
        CastleQueenside = '2'
    }

    public enum TypeOfPromotion
    {
        NoPromotion = 0,
        PromoteBishop = 'B',
        PromoteKnight = 'N',
        PromoteRook = 'R',
        PromoteQueen = 'Q'
    }
}