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
                TypeOfMove.Move => IndexToUCI(move.From) + IndexToUCI(move.To) + promotionSuffix,
                TypeOfMove.Take => IndexToUCI(move.From) + IndexToUCI(move.To) + promotionSuffix,
                _ => throw new NotImplementedException(),
            };

        }

        public static string IndexToUCI(int index) => new string(new char[] { (char)((index % 8) + 'a'), (char)((index / 8) + '1') });

        public static int UCIToIndex(string a1) => UCIToIndex(a1[0], a1[1]);
        public static int UCIToIndex(char col, char row) => (col - 'a') + 8 * (row - '1');

        public static Move DeserializeMove(string uic)
        {
            // check format
            if (uic.Length == 4)
                return new Move(UCIToIndex(uic[0], uic[1]), UCIToIndex(uic[2], uic[3]), TypeOfMove.Move);

            if (uic.Length == 5)
            {
                if (uic[2] == 'x')
                    return new Move(UCIToIndex(uic[0], uic[1]), UCIToIndex(uic[3], uic[4]), TypeOfMove.Take);

                TypeOfPromotion promotion = uic[4] switch
                {
                    'Q' => TypeOfPromotion.PromoteQueen,
                    'R' => TypeOfPromotion.PromoteRook,
                    'N' => TypeOfPromotion.PromoteKnight,
                    'B' => TypeOfPromotion.PromoteBishop,
                    _ => throw new ArgumentException("Cannot understand promotion char " + uic[4])
                };

                return new Move(UCIToIndex(uic[0], uic[1]), UCIToIndex(uic[2], uic[3]), TypeOfMove.Move, promotion);
            }

            throw new ArgumentException("uic has wrong length of " + uic.Length);
        }
    }
}
