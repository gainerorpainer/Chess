using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.XPath;

namespace ChEngine
{
    public class Board : ICloneable
    {
        public Field[] Fields;
        public PlayerOption[] PlayerOptions;
        public bool IsWhiteToMove;

        private Board()
        { }

        public Board(IEnumerable<Move> moves)
        {
            Fields = (Field[])Rules.DEFAULT_FIELDS.Clone();
            IsWhiteToMove = true;
            PlayerOptions = (PlayerOption[])Rules.DEFAULT_PLAYER_OPTIONS.Clone();

            // Apply all mutations
            foreach (var move in moves)
                Mutate(move);
        }

        public object Clone()
        {
            return new Board()
            {
                Fields = (Field[])Fields.Clone(),
                PlayerOptions = (PlayerOption[])PlayerOptions.Clone(),
                IsWhiteToMove = IsWhiteToMove
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Board b)
                return Enumerable.SequenceEqual(b.Fields, Fields)
                    && (b.IsWhiteToMove && IsWhiteToMove)
                    && Enumerable.SequenceEqual(b.PlayerOptions, PlayerOptions);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                IsWhiteToMove,
                Enumerable.Aggregate(Fields, 0, (last, next) => HashCode.Combine(
                    last,
                    next.GetHashCode()
                )),
                Enumerable.Aggregate(PlayerOptions, 0, (last, next) => HashCode.Combine(
                    last,
                    next.GetHashCode()
                )));
        }


        public void Mutate(Move move)
        {
            switch (move.Type)
            {
                case TypeOfMove.Move:
                case TypeOfMove.Take:

                    // Pick piece
                    Field figureFrom = Fields[move.From];

                    // This could be special move (king castles) or lose the right to castle
                    if (figureFrom.Figure == TypeOfFigure.King)
                    {
                        // this will lose the right to castle always
                        PlayerOptions[CurrentPlayerId()].KingsideCastle = PlayerOptions[CurrentPlayerId()].QueensideCastle = false;

                        // King can normally never jump two!    
                        int colJump = move.To % 8 - move.From % 8;
                        int rowNumber = move.To / 8;

                        // kingside
                        if (colJump == 2)
                        {
                            // Note: there must be several conditions met such that this mutation is possible!
                            // teleport rook here
                            Fields[7 + rowNumber * 8].Figure = TypeOfFigure.EMPTY;
                            Fields[5 + rowNumber * 8].Figure = TypeOfFigure.Rook;
                            Fields[5 + rowNumber * 8].IsWhite = IsWhiteToMove;
                        }
                        // queenside
                        else if (colJump == -2)
                        {
                            // teleport rook here
                            Fields[0 + rowNumber * 8].Figure = TypeOfFigure.EMPTY;
                            Fields[3 + rowNumber * 8].Figure = TypeOfFigure.Rook;
                            Fields[3 + rowNumber * 8].IsWhite = IsWhiteToMove;
                        }
                    }

                    // this could be special move pawn long jump
                    if ((figureFrom.Figure == TypeOfFigure.Pawn) && (Math.Abs(move.From - move.To) == 2 * 8))
                    {
                        // add a en passant taking option for the enemy
                        PlayerOptions[OtherPlayerId()].GiveEnpassantOnCol(move.From % 8);
                    }

                    // this rook move could loose the right to castle
                    if (figureFrom.Figure == TypeOfFigure.Rook)
                    {
                        // this can overwrite multiple times, which is not bad just maybe not optimal
                        int colNum = move.From % 8;
                        if (colNum == 0)
                            PlayerOptions[CurrentPlayerId()].QueensideCastle = false;
                        else if (colNum == 7)
                            PlayerOptions[CurrentPlayerId()].KingsideCastle = false;
                    }

                    // remove from
                    Fields[move.From].Figure = TypeOfFigure.EMPTY;

                    // Check destination
                    Fields[move.To] = figureFrom;
                    break;

                case TypeOfMove.NullMove:
                    // Do nothing actually
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (move.Promotion)
            {
                case TypeOfPromotion.NoPromotion:
                    break;

                case TypeOfPromotion.PromoteKnight:
                case TypeOfPromotion.PromoteBishop:
                case TypeOfPromotion.PromoteRook:
                case TypeOfPromotion.PromoteQueen:
                    // Get type
                    var pieceType = (TypeOfFigure)move.Promotion;

                    // place piece there
                    Fields[move.To].Figure = pieceType;

                    break;

                default:
                    throw new NotImplementedException();
            }

            // Revoke all options for en passant
            PlayerOptions[CurrentPlayerId()].ClearEnpassant();

            // Flip who is to move
            IsWhiteToMove = !IsWhiteToMove;
        }


        public int CurrentPlayerId() => PlayerId(IsWhiteToMove);
        public int OtherPlayerId() => PlayerId(!IsWhiteToMove);
        static int PlayerId(bool isWhite) => isWhite ? 0 : 1;

        public List<Move> GetLegalMoves()
        {
            List<Move> moves = GetMoves_IgnoreCheckRules().ToList();

            // castling?
            // check that you are not in check!
            if (GetInCheck() == false)
            {
                int kingPos = 4 + (IsWhiteToMove ? 0 : 7) * 8;
                if (PlayerOptions[CurrentPlayerId()].KingsideCastle)
                {
                    if (CheckTemporaryCastlingCondition(kingPos, towardsKingside: true))
                        // if you got here, nice!
                        moves.Add(new Move(kingPos, kingPos + 2, TypeOfMove.Move));
                }

                if (PlayerOptions[CurrentPlayerId()].QueensideCastle)
                {
                    if (CheckTemporaryCastlingCondition(kingPos, towardsKingside: false))
                        // if you got here, nice!
                        moves.Add(new Move(kingPos, kingPos - 2, TypeOfMove.Move));
                }
            }

            // you are never allowed to make a move which would make it possible for your enemy to take your king on his next move!
            // This automatically includes 'check' and 'pinned pieces' rule
            List<Move> protectKingRule = new List<Move>();

            // find your king here
            int kingLocation = FindInArray(Fields, x => x.Figure == TypeOfFigure.King && x.IsWhite == IsWhiteToMove);

            // todo: maybe easiest is to make a copy for each legal move and see if you can take the king
            foreach (var move in moves)
            {
                var clone = (Board)Clone();
                clone.Mutate(move);

                // check if you need to recalculate king location
                int possiblyNewKingLocation = move.From != kingLocation ? kingLocation : move.To;

                // Check enemy moves
                var couldTakeKing = clone.GetMoves_IgnoreCheckRules().Any(x => x.Type == TypeOfMove.Take && x.To == possiblyNewKingLocation);
                if (couldTakeKing)
                    continue;

                // This move is ok
                protectKingRule.Add(move);
            }

            moves = protectKingRule;

            return moves;
        }

        /// <summary>
        /// Returns an iterator for looking for simple moves
        /// These are moves that you can take by applying the normal move rules for pieces
        /// Like: Empty space, takes, etc
        /// Does not take check or pinning rules into consideration
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Move> GetMoves_IgnoreCheckRules()
        {
            // Find each figure
            for (int i = 0; i < 8 * 8; i++)
            {
                Field f = Fields[i];

                // XOR is same as saying that the color is not who is to move
                if (f.IsWhite ^ IsWhiteToMove)
                    continue;

                switch (f.Figure)
                {
                    case TypeOfFigure.EMPTY:
                        break;

                    case TypeOfFigure.Rook:
                        foreach (var item in RooklikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Knight:
                        foreach (var item in KnightlikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Bishop:
                        foreach (var item in BishoplikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Queen:
                        foreach (var item in RooklikeMoves(i))
                            yield return item;
                        foreach (var item in BishoplikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.King:
                        foreach (var item in KingMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Pawn:
                        foreach (var item in PawnMoves(i))
                            yield return item;
                        break;

                    default:
                        break;
                }
            }
        }

        public bool GetInCheck()
        {
            // find your king here
            int kingLocation = FindInArray(Fields, x => x.Figure == TypeOfFigure.King && x.IsWhite == IsWhiteToMove);

            // make a copy
            var clone = (Board)Clone();

            // make a null move (to give the other player the move)
            clone.Mutate(Move.NullMove);

            // Check enemy moves
            var result = clone.GetMoves_IgnoreCheckRules().Any(x => x.Type == TypeOfMove.Take && x.To == kingLocation);

            return result;
        }

        private int FindInArray<T>(T[] arr, Func<T, bool> predicate)
        {
            for (int i = 0; i < arr.Length; i++)
                if (predicate(arr[i]))
                    return i;

            return -1;
        }

        private IEnumerable<Move> PawnMoves(int i)
        {
            int rowNum = i / 8;
            bool isWhite = Fields[i].IsWhite;
            int oneFromLastRow = isWhite ? 6 : 1;
            int sign = isWhite ? 1 : -1;


            // can go one up if free
            int nextField = i + sign * 8;
            if (Fields[nextField].Figure == TypeOfFigure.EMPTY)
            {
                if (rowNum == oneFromLastRow)
                {
                    // promote if last line
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                }
                else
                {
                    yield return new Move(i, nextField, TypeOfMove.Move);

                    // can jump 2 times if both are free and is on first pawn line
                    if (rowNum == (isWhite ? 1 : 7))
                    {
                        nextField += sign * 8;
                        if (Fields[nextField].Figure == TypeOfFigure.EMPTY)
                            yield return new Move(i, nextField, TypeOfMove.Move);
                    }
                }
            }

            int enPassantRow = isWhite ? 4 : 3;

            // Can take diagonally left
            int colNum = i % 8;
            if (colNum > 0)
            {
                // note, this is left regardless if black or white!
                int dest = i - 1 + sign * 8;
                if (
                    (Fields[dest].Figure != TypeOfFigure.EMPTY)
                    &&
                    (Fields[dest].IsWhite != IsWhiteToMove)
                )
                {
                    if (rowNum != oneFromLastRow)
                    {
                        // normal move forward
                        yield return new Move(i, dest, TypeOfMove.Take);
                    }
                    // promote if last line
                    else
                    {
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                    }
                }

                // Check en passant
                if ((rowNum == enPassantRow) && PlayerOptions[CurrentPlayerId()].CheckEnpassantOnCol(colNum - 1))
                    yield return new Move(i, dest, TypeOfMove.Take);
            }

            // and right
            if (colNum < 7)
            {
                // Note: this is right regardless if black or white
                int dest = i + 1 + sign * 8;
                if (
                    (Fields[dest].Figure != TypeOfFigure.EMPTY)
                    &&
                    (Fields[dest].IsWhite != IsWhiteToMove)
                )
                {
                    if (rowNum != oneFromLastRow)
                    {
                        yield return new Move(i, dest, TypeOfMove.Take);
                    }
                    // And promote if last line
                    else
                    {
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                    }
                }

                // Check en passant
                if ((rowNum == enPassantRow) && PlayerOptions[CurrentPlayerId()].CheckEnpassantOnCol(colNum + 1))
                    yield return new Move(i, dest, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> KingMoves(int from)
        {
            // iterate over each
            foreach (var index in Rules.LOOKUP_KINGMOVES[from])
            {
                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(from, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(from, index, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> KnightlikeMoves(int from)
        {
            // iterate over possibilities
            foreach (var index in Rules.LOOKUP_KNIGHTMOVES[from])
            {
                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(from, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(from, index, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> BishoplikeMoves(int i)
        {
            // Store some vars
            int colNum = i % 8;
            int rowNum = i / 8;
            int columnsToRight = 8 - (colNum + 1);
            int rowsToTop = 8 - (rowNum + 1);

            int index;

            // Go right and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowsToTop); times++)
            {
                index += 8 + 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go left and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowsToTop); times++)
            {
                index += 8 - 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go left and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowNum); times++)
            {
                index += -8 - 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go right and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowNum); times++)
            {
                index += -8 + 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }
        }

        private IEnumerable<Move> RooklikeMoves(int i)
        {
            // Store some vars
            int colNum = i % 8;
            int rowNum = i / 8;
            int columnOffset = colNum;
            int rowOffset = (i / 8) * 8;

            // Go right until you hit something
            for (int colI = columnOffset + 1; colI < 8; colI++)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // Go left until you hit something
            for (int colI = columnOffset - 1; colI >= 0; colI--)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // Go up until you hit something
            for (int rowI = rowNum + 1; rowI < 8; rowI++)
            {
                int index = columnOffset + (rowI * 8);
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // go down until you hit something
            for (int rowI = rowNum - 1; rowI >= 0; rowI--)
            {
                int index = columnOffset + (rowI * 8);
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }
        }

        public bool CheckTemporaryCastlingCondition(int kingPos, bool towardsKingside)
        {
            int sign = towardsKingside ? 1 : -1;
            int rowOffset = (kingPos / 8) * 8; // note the integer division!

            // first of all, all cols to the corner must be free
            for (int i = 4 + sign; (i > 0) && (i < 7); i += sign)
            {
                if (Fields[i + rowOffset].Figure != TypeOfFigure.EMPTY)
                    return false;
            }

            // each subsequent move must not result in check
            Board copy = (Board)Clone();

            for (int i = 0; i < 2; i++)
            {
                // move the king one square
                copy.Mutate(new Move(kingPos + sign * i, kingPos + sign * (i + 1), TypeOfMove.Move));

                // Make a null move such that it is the same player again
                copy.Mutate(Move.NullMove);

                // you cannot be in check!
                if (copy.GetInCheck())
                    return false;
            }

            return true;
        }
    }
}