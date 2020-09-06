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

        public override string ToString()
        {
            return UnderlyingMove.MoveType switch
            {
                MoveType.Move => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To),
                MoveType.Take => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To),
                MoveType.CastleKingside => "0-0",
                MoveType.CastleQueenside => "0-0-0",
                MoveType.PromoteKnight => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To) + "N",
                MoveType.PromoteBishop => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To) + "B",
                MoveType.PromoteRook => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To) + "R",
                MoveType.PromoteQueen => IToStr(UnderlyingMove.From) + IToStr(UnderlyingMove.To) + "Q",
                _ => throw new NotImplementedException(),
            };

            static string IToStr(int index) => new string(new char[] { (char)((index % 8) + 'a'), (char)((index / 8) + '1') });
        }

        public static Move ParseMove(string uic)
        {
            // check format
            if (uic.Length == 4)
                return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[2] - 'a') + 8 * (uic[3] - '1'), MoveType.Move);

            if (uic.Length == 5)
            {
                if (uic[2] == 'x')
                    return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[3] - 'a') + 8 * (uic[4] - '1'), MoveType.Take);

                MoveType moveType = uic[4] switch
                {
                    'Q' => MoveType.PromoteQueen,
                    'R' => MoveType.PromoteRook,
                    'N' => MoveType.PromoteKnight,
                    'B' => MoveType.PromoteBishop,
                    _ => throw new ArgumentException("Cannot understand promotion char " + uic[4])
                };

                return new Move((uic[0] - 'a') + 8 * (uic[1] - '1'), (uic[2] - 'a') + 8 * (uic[3] - '1'), moveType);
            }

            throw new ArgumentException("uic has wrong length of " + uic.Length);
        }
    }
}
