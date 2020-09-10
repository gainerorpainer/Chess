namespace ChEngine
{

    public struct Move
    {
        public int From;
        public int To;
        public TypeOfMove Type;
        public TypeOfPromotion Promotion;

        public Move(int from, int to, TypeOfMove moveType, TypeOfPromotion promotion = TypeOfPromotion.NoPromotion)
        {
            From = from;
            To = to;
            Type = moveType;
            Promotion = promotion;
        }
    }
}