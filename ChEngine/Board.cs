using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace ChEngine
{
    public class Board : ICloneable
    {
        public Field[] Fields;
        public bool IsWhiteToMove;
        public PlayerOption[] PlayerOptions;
        public readonly Cache Cache = new Cache();

        public static readonly Field[] DefaultField = CreateDefaultField();
        private static Field[] CreateDefaultField()
        {
            var result = new Field[8 * 8];

            // Standard config: pawns
            for (int i = 8; i < 8 + 8; i++)
            {
                result[i] = new Field(true, TypeOfFigure.Pawn);
                result[i + (5 * 8)] = new Field(false, TypeOfFigure.Pawn);
            }

            const int sevenRows = 7 * 8;

            // standard config: pieces
            result[0] = new Field(true, TypeOfFigure.Rook);
            result[0 + sevenRows] = new Field(false, TypeOfFigure.Rook);

            result[1] = new Field(true, TypeOfFigure.Knight);
            result[1 + sevenRows] = new Field(false, TypeOfFigure.Knight);

            result[2] = new Field(true, TypeOfFigure.Bishop);
            result[2 + sevenRows] = new Field(false, TypeOfFigure.Bishop);

            result[3] = new Field(true, TypeOfFigure.Queen);
            result[3 + sevenRows] = new Field(false, TypeOfFigure.Queen);

            result[4] = new Field(true, TypeOfFigure.King);
            result[4 + sevenRows] = new Field(false, TypeOfFigure.King);

            result[5] = new Field(true, TypeOfFigure.Bishop);
            result[5 + sevenRows] = new Field(false, TypeOfFigure.Bishop);

            result[6] = new Field(true, TypeOfFigure.Knight);
            result[6 + sevenRows] = new Field(false, TypeOfFigure.Knight);

            result[7] = new Field(true, TypeOfFigure.Rook);
            result[7 + sevenRows] = new Field(false, TypeOfFigure.Rook);

            return result;
        }

        private Board()
        { }

        public Board(IEnumerable<Move> moves)
        {
            Fields = (Field[])DefaultField.Clone();
            IsWhiteToMove = true;
            PlayerOptions = new PlayerOption[2];

            // Apply all mutations
            foreach (var move in moves)
                Mutate(move);
        }

        public object Clone()
        {
            return new Board()
            {
                Fields = (Field[])Fields.Clone(),
                IsWhiteToMove = IsWhiteToMove,
                PlayerOptions = (PlayerOption[])PlayerOptions.Clone()
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
                    Field figureTo = Fields[move.To];
                    Fields[move.To] = figureFrom;

                    // Revoke all options for en passant
                    PlayerOptions[CurrentPlayerId()].ClearEnpassant();

                    // Flip who is to move
                    IsWhiteToMove = !IsWhiteToMove;

                    // Disvalidate Cache
                    Cache.Clear();
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
        }


        public int CurrentPlayerId() => PlayerId(IsWhiteToMove);
        public int OtherPlayerId() => PlayerId(!IsWhiteToMove);
        public static int PlayerId(bool isWhite) => isWhite ? 0 : 1;

        public List<Move> GetLegalMoves()
        {
            // Check Cache
            if (Cache.LegalMoves != null)
                return Cache.LegalMoves;

            List<Move> moves = GetMoves_IgnoreCheckRules().ToList();

            // you are never allowed to make a move which would make it possible for your enemy to take your king on his next move!
            // This automatically includes 'check' and 'pinned pieces' rule
            List<Move> protectKingRule = new List<Move>();

            // find your king here
            int kingLocation = Fields.ToList().FindIndex(x => x.Figure == TypeOfFigure.King && x.IsWhite == IsWhiteToMove);

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

            // Store cache
            Cache.LegalMoves = moves;

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

        private IEnumerable<Move> PawnMoves(int i)
        {
            int rowNum = i / 8;
            bool isWhite = Fields[i].IsWhite;
            int oneFromLastRow = isWhite ? 7 : 1;
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
                        nextField = nextField + sign * 8;
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

        private IEnumerable<Move> KingMoves(int i)
        {
            // There are 8 configurations
            List<Point> vectors = new List<Point>() {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(1, -1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(-1, 0),
                new Point(-1, 1),
            };

            // Check bounds
            int colNumber = i % 8;
            int rowNumber = i / 8;
            // check to right / left
            switch (colNumber)
            {
                case 0:
                    // remove all possibilites that go left
                    vectors.RemoveAll(x => x.X < 0);
                    break;
                case 7:
                    // remove all possibilites that go right 
                    vectors.RemoveAll(x => x.X > 0);
                    break;
                default:
                    break;
            }

            // check to top / bottom
            switch (rowNumber)
            {
                case 0:
                    // remove all possibilites that go bottom
                    vectors.RemoveAll(x => x.Y < 0);
                    break;
                case 7:
                    // remove all possibilites that go top 
                    vectors.RemoveAll(x => x.Y > 0);
                    break;
                default:
                    break;
            }

            // iterate over each
            foreach (var vec in vectors)
            {
                int index = i + vec.X + (vec.Y * 8);

                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(i, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(i, index, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> KnightlikeMoves(int i)
        {
            // There are 8 configurations
            List<Point> vectors = new List<Point>() {
                new Point(1, 2),
                new Point(2, 1),
                new Point(2, -1),
                new Point(1, -2),
                new Point(-1, -2),
                new Point(-2, -1),
                new Point(-2, 1),
                new Point(-1, 2),
            };

            int colNumber = i % 8;
            int rowNumber = i / 8;

            // check to right / left
            switch (colNumber)
            {
                case 0:
                    // remove all possibilites that go left at all
                    vectors.RemoveAll(x => x.X < 0);
                    break;
                case 1:
                    // remove all possibilites to go left 2 times
                    vectors.RemoveAll(x => x.X == -2);
                    break;
                case 6:
                    // remove all possibilites to go right 2 times
                    vectors.RemoveAll(x => x.X == 2);
                    break;
                case 7:
                    // remove all possibilites that go right at all 
                    vectors.RemoveAll(x => x.X > 0);
                    break;
                default:
                    break;
            }

            // check to top / bottom
            switch (rowNumber)
            {
                case 0:
                    // remove all possibilites that go bottom at all
                    vectors.RemoveAll(x => x.Y < 0);
                    break;
                case 1:
                    // remove all possibilites to go bottom 2 times
                    vectors.RemoveAll(x => x.Y == -2);
                    break;
                case 6:
                    // remove all possibilites to go top 2 times
                    vectors.RemoveAll(x => x.Y == 2);
                    break;
                case 7:
                    // remove all possibilites that go top at all 
                    vectors.RemoveAll(x => x.Y > 0);
                    break;
                default:
                    break;
            }

            // iterate over leftovers
            foreach (var vec in vectors)
            {
                int index = i + vec.X + (vec.Y * 8);

                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(i, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(i, index, TypeOfMove.Take);
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

        public double GetEvaluation()
        {
            // Check cache
            if (Cache.Evaluation != null)
                return Cache.Evaluation.Value;

            // simply count pieces
            double score = 0;
            for (int i = 0; i < 8 * 8; i++)
                score += Weighting(Fields[i].Figure) * (Fields[i].IsWhite ? 1 : -1);


            // store cache
            Cache.Evaluation = score;

            return score;
        }

        public static double Weighting(TypeOfFigure type)
        {
            return type switch
            {
                TypeOfFigure.EMPTY => 0,
                TypeOfFigure.Rook => 5,
                TypeOfFigure.Knight => 3,
                TypeOfFigure.Bishop => 3,
                TypeOfFigure.Queen => 9,
                TypeOfFigure.King => 50,
                TypeOfFigure.Pawn => 1,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public class Cache
    {
        public List<Move> LegalMoves { get; set; }
        public double? Evaluation { get; set; }

        public void Clear()
        {
            LegalMoves = null;
            Evaluation = null;
        }
    }

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