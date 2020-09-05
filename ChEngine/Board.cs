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

        public Board(string moves)
        {
            Fields = (Field[])DefaultField.Clone();
            IsWhiteToMove = true;

            // Apply all mutations
            string[] movesSplitted = moves.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var move in movesSplitted)
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


        internal void Mutate(string move)
        {
            // Pick piece
            Move parsedMove = ParseMove(move);
            Field figureFrom = Fields[parsedMove.From];


            // remove from
            Fields[parsedMove.From].Figure = FigureType.EMPTY;

            // Check destination
            Field figureTo = Fields[parsedMove.To];
            Fields[parsedMove.To] = figureFrom;

            // Flip who is to move
            IsWhiteToMove = !IsWhiteToMove;

            // Disvalidate Cache
            Cache.Clear();
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

        public static string GetUCI(int from, int to, MoveType type)
        {
            return type switch
            {
                MoveType.Move => IToStr(from) + IToStr(to),
                MoveType.Take => IToStr(from) + 'x' + IToStr(to),
                MoveType.CastleKingside => "0-0",
                MoveType.CastleQueenside => "0-0-0",
                MoveType.PromoteKnight => IToStr(from) + IToStr(to) + "N",
                MoveType.PromoteBishop => IToStr(from) + IToStr(to) + "B",
                MoveType.PromoteRook => IToStr(from) + IToStr(to) + "R",
                MoveType.PromoteQueen => IToStr(from) + IToStr(to) + "Q",
                _ => throw new NotImplementedException(),
            };

            static string IToStr(int index) => new string(new char[] { (char)((index % 8) + 'a'), (char)((index / 8) + '1') });
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

        public List<string> GetLegalMoves()
        {
            // Check Cache
            if (Cache.LegalMoves != null)
                return Cache.LegalMoves;

            List<string> moves = new List<string>();

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

        private void AddPawnlikeMoves(List<string> moves, int i)
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
                            moves.Add(GetUCI(i, i + 8, MoveType.Move));

                            // can jump 2 times
                            // but only of both fields are free
                            if (Fields[i + 2 * 8].Figure == FigureType.EMPTY)
                                moves.Add(GetUCI(i, i + 2 * 8, MoveType.Move));
                        }
                        break;

                    default:
                        // can go one up if free
                        if (Fields[i + 8].Figure == FigureType.EMPTY)
                            moves.Add(GetUCI(i, i + 8, MoveType.Move));
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
                        moves.Add(GetUCI(i, dest, MoveType.Take));
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
                        moves.Add(GetUCI(i, dest, MoveType.Take));
                }
            }
        }

        private void AddKinglikeMoves(List<string> moves, int i)
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

        private void AddKnightlikeMoves(List<string> moves, int i)
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

        private void AddIfEmptyOrEnemy(List<string> moves, int from, int to)
        {
            if (Fields[to].Figure == FigureType.EMPTY)
                moves.Add(GetUCI(from, to, MoveType.Move));
            else if (Fields[to].IsWhite ^ IsWhiteToMove)
                moves.Add(GetUCI(from, to, MoveType.Take));
        }

        private void AddBishoplikeMoves(List<string> moves, int i)
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
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
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
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
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
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
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
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
            }
        }

        private void AddRooklikeMoves(List<string> moves, int i)
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
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
            }

            // Go left until you hit something
            for (int colI = columnOffset - 1; colI >= 0; colI--)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
            }

            // Go up until you hit something
            for (int rowI = rowNum + 1; rowI < 8; rowI++)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
            }

            // go down until you hit something
            for (int rowI = rowNum - 1; rowI >= 0; rowI--)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Figure != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUCI(i, index, MoveType.Take));

                    break;
                }

                moves.Add(GetUCI(i, index, MoveType.Move));
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

    public enum GameState
    {
        NDEF,
        Normal,
        Check,
        Checkmate
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


    public class Cache
    {
        public List<string> LegalMoves { get; set; }
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