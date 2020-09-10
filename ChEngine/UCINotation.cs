using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine
{
    public class UCINotation
    {
        public Move UnderlyingMove { get; private set; }

        public UCINotation(Move underlyingMove)
        {
            UnderlyingMove = underlyingMove;
        }

        public UCINotation(string str)
        {
            UnderlyingMove = DeserializeMove(str);
        }

        public override string ToString() => SerializeMove(UnderlyingMove);

        public static string SerializeMove(Move move)
        {
            string promotionSuffix = move.Promotion == TypeOfPromotion.NoPromotion ? "" : ((char)move.Promotion).ToString();
            return move.Type switch
            {
                TypeOfMove.Move => IToStr(move.From) + IToStr(move.To) + promotionSuffix,
                TypeOfMove.Take => IToStr(move.From) + IToStr(move.To) + promotionSuffix,
                TypeOfMove.CastleKingside => "0-0",
                TypeOfMove.CastleQueenside => "0-0-0",
                _ => throw new NotImplementedException(),
            };

        }
        public static string IToStr(int index) => new string(new char[] { (char)((index % 8) + 'a'), (char)((index / 8) + '1') });

        public static Move DeserializeMove(string uic)
        {
            // check format
            if (uic.Length == 4)
                return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[2] - 'a') + 8 * (uic[3] - '1'), TypeOfMove.Move);

            if (uic.Length == 5)
            {
                if (uic[2] == 'x')
                    return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[3] - 'a') + 8 * (uic[4] - '1'), TypeOfMove.Take);

                TypeOfPromotion promotion = uic[4] switch
                {
                    'Q' => TypeOfPromotion.PromoteQueen,
                    'R' => TypeOfPromotion.PromoteRook,
                    'N' => TypeOfPromotion.PromoteKnight,
                    'B' => TypeOfPromotion.PromoteBishop,
                    _ => throw new ArgumentException("Cannot understand promotion char " + uic[4])
                };

                return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[2] - 'a') + 8 * (uic[3] - '1'), TypeOfMove.Move, promotion);
            }

            throw new ArgumentException("uic has wrong length of " + uic.Length);
        }
    }
}
