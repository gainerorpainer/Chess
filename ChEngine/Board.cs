using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public readonly Cache Cache = new Cache();

        public static readonly Field[] DefaultField = CreateDefaultField();

        private static Field[] CreateDefaultField()
        {
            var result = new Field[8 * 8];

            // Standard config: pawns
            for (int i = 8; i < 8 + 8; i++)
            {
                result[i] = new Field(true, FigureType.Pawn);
                result[i + 5 * 8] = new Field(false, FigureType.Pawn);
            }

            const int sevenRows = 7 * 8;

            // standard config: pieces
            result[0] = new Field(true, FigureType.Rook);
            result[0 + sevenRows] = new Field(false, FigureType.Rook);

            result[1] = new Field(true, FigureType.Knight);
            result[1 + sevenRows] = new Field(false, FigureType.Knight);

            result[2] = new Field(true, FigureType.Bishop);
            result[2 + sevenRows] = new Field(false, FigureType.Bishop);

            result[3] = new Field(true, FigureType.Queen);
            result[3 + sevenRows] = new Field(false, FigureType.Queen);

            result[4] = new Field(true, FigureType.King);
            result[4 + sevenRows] = new Field(false, FigureType.King);

            result[5] = new Field(true, FigureType.Bishop);
            result[5 + sevenRows] = new Field(false, FigureType.Bishop);

            result[6] = new Field(true, FigureType.Knight);
            result[6 + sevenRows] = new Field(false, FigureType.Knight);

            result[7] = new Field(true, FigureType.Rook);
            result[7 + sevenRows] = new Field(false, FigureType.Rook);

            return result;
        }

        private Board()
        { }

        public Board(IEnumerable<Move> moves)
        {
            Fields = (Field[])DefaultField.Clone();
            IsWhiteToMove = true;

            // Apply all mutations
            foreach (var move in moves)
                Mutate(move);
        }

        public object Clone()
        {
            return new Board()
            {
                Fields = (Field[])Fields.Clone(),
                IsWhiteToMove = IsWhiteToMove
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Board b)
                return Enumerable.SequenceEqual(b.Fields, Fields) && (b.IsWhiteToMove && IsWhiteToMove);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                IsWhiteToMove,
                Enumerable.Aggregate(Fields, 0, (last, next) => HashCode.Combine(
                    last,
                    next.GetHashCode()
                )));
        }


        internal void Mutate(Move move)
        {
            switch (move.MoveType)
            {
                case MoveType.Move:
                case MoveType.Take:
                    // Pick piece
                    Field figureFrom = Fields[move.From];


                    // remove from
                    Fields[move.From].Figure = FigureType.EMPTY;

                    // Check destination
                    Field figureTo = Fields[move.To];
                    Fields[move.To] = figureFrom;

                    // Flip who is to move
                    IsWhiteToMove = !IsWhiteToMove;

                    // Disvalidate Cache
                    Cache.Clear();
                    break;

                    
                case MoveType.CastleKingside:
                    
                case MoveType.CastleQueenside:
                    
                case MoveType.PromoteKnight:
                    
                case MoveType.PromoteBishop:
                    
                case MoveType.PromoteRook:
                    
                case MoveType.PromoteQueen:
                    
                default:
                    throw new NotImplementedException();
            }
        }

        public GameState GetBoardState()
        {
            // Check Cache
            if (Cache.BoardState != null)
                return Cache.BoardState.Value;

            GameState result = default;

            if (GetLegalMoves().Count == 0)
                result = GameState.Checkmate;

            // Store cache
            Cache.BoardState = result;

            return result;
        }

        public List<Move> GetLegalMoves()
        {
            // Check Cache
            if (Cache.LegalMoves != null)
                return Cache.LegalMoves;

            List<Move> moves = new List<Move>();

            // Find each figure
            for (int i = 0; i < 8 * 8; i++)
            {
                Field f = Fields[i];

                // XOR is same as saying that the color is not who is to move
                if (f.IsWhite ^ IsWhiteToMove)
                    continue;

                switch (f.Figure)
                {
                    case FigureType.EMPTY:
                        break;
                    case FigureType.Rook:
                        AddRooklikeMoves(moves, i);
                        break;
                    case FigureType.Knight:
                        AddKnightlikeMoves(moves, i);
                        break;
                    case FigureType.Bishop:
                        AddBishoplikeMoves(moves, i);
                        break;
                    case FigureType.Queen:
                        AddRooklikeMoves(moves, i);
                        AddBishoplikeMoves(moves, i);
                        break;
                    case FigureType.King:
                        AddKinglikeMoves(moves, i);
                        break;
                    case FigureType.Pawn:
                        AddPawnlikeMoves(moves, i);
                        break;
                    default:
                        break;
                }
            }

            // Store cache
            Cache.LegalMoves = moves;

            return moves;
        }

        private void AddPawnlikeMoves(List<Move> moves, int i)
        {
            int rowNum = i / 8;
            if (Fields[i].IsWhite)
            {
                switch (rowNum)
                {
                    case 6:
                        // can promote

                        break;

                    case 1:
                        // can jump one if free
                        if (Fields[i + 8].Figure == FigureType.EMPTY)
                        {
                            moves.Add(new Move(i, i + 8, MoveType.Move));

                            // can jump 2 times
                            // but only of both fields are free
                            if (Fields[i + 2 * 8].Figure == FigureType.EMPTY)
                                moves.Add(new Move(i, i + 2 * 8, MoveType.Move));
                        }
                        break;

                    default:
                        // can go one up if free
                        if (Fields[i + 8].Figure == FigureType.EMPTY)
                            moves.Add(new Move(i, i + 8, MoveType.Move));
                        break;
                }

                // Can take diagonally left
                int colNum = i % 8;
                if (colNum > 0)
                {
                    int dest = i - 1 + 1 * 8;
                    if (
                        (Fields[dest].Figure != FigureType.EMPTY)
                        &&
                        (Fields[dest].IsWhite != IsWhiteToMove)
                    )
                        moves.Add(new Move(i, dest, MoveType.Take));
                }

                // and right
                if (colNum < 7)
                {
                    int dest = i + 1 + 1 * 8;
                    if (
                        (Fields[dest].Figure != FigureType.EMPTY)
                        &&
                        (Fields[dest].IsWhite != IsWhiteToMove)
                    )
                        moves.Add(new Move(i, dest, MoveType.Take));
                }
            }
        }

        private void AddKinglikeMoves(List<Move> moves, int i)
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
                int index = i + vec.X + vec.Y * 8;
                AddIfEmptyOrEnemy(moves, i, index);
            }
        }

        private void AddKnightlikeMoves(List<Move> moves, int i)
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
                int index = i + vec.X + vec.Y * 8;
                AddIfEmptyOrEnemy(moves, i, index);
            }
        }

        private void AddIfEmptyOrEnemy(List<Move> moves, int from, int to)
        {
            if (Fields[to].Figure == FigureType.EMPTY)
                moves.Add(new Move(from, to, MoveType.Move));
            else if (Fields[to].IsWhite ^ IsWhiteToMove)
                moves.Add(new Move(from, to, MoveType.Take));
        }

        private void AddBishoplikeMoves(List<Move> moves, int i)
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
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // Go left and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowsToTop); times++)
            {
                index += 8 - 1;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // Go left and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowNum); times++)
            {
                index += -8 - 1;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // Go right and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowNum); times++)
            {
                index += -8 + 1;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }
        }

        private void AddRooklikeMoves(List<Move> moves, int i)
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
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // Go left until you hit something
            for (int colI = columnOffset - 1; colI >= 0; colI--)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // Go up until you hit something
            for (int rowI = rowNum + 1; rowI < 8; rowI++)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
            }

            // go down until you hit something
            for (int rowI = rowNum - 1; rowI >= 0; rowI--)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(new Move(i, index, MoveType.Take));

                    break;
                }

                moves.Add(new Move(i, index, MoveType.Move));
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

        public static double Weighting(FigureType type)
        {
            return type switch
            {
                FigureType.EMPTY => 0,
                FigureType.Rook => 5,
                FigureType.Knight => 3,
                FigureType.Bishop => 3,
                FigureType.Queen => 9,
                FigureType.King => 50,
                FigureType.Pawn => 1,
                _ => throw new NotImplementedException(),
            };
        }
    }



    public enum GameState
    {
        NDEF,
        Normal,
        Check,
        Checkmate
    }


    public class Cache
    {
        public List<Move> LegalMoves { get; set; }
        public double? Evaluation { get; set; }
        public GameState? BoardState { get; set; }

        public void Clear()
        {
            LegalMoves = null;
            Evaluation = null;
            BoardState = null;
        }
    }
}