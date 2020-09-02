using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace ChEngine
{
    enum AllowedGameState
    {
        Normal,
        Check,
        Checkmate
    }

    public struct Board
    {
        public Figure[] Fields;
        public bool IsWhiteToMove;
        public Cache Cache;

        public Board(string moves)
        {
            Fields = new Figure[8 * 8];
            Cache = null;

            // Standard config: pawns
            for (int i = 8; i < 8 + 8; i++)
            {
                Fields[i] = new Figure(true, FigureType.Pawn);
                Fields[i + 5 * 8] = new Figure(false, FigureType.Pawn);
            }

            const int sevenRows = 7 * 8;

            // standard config: pieces
            Fields[0] = new Figure(true, FigureType.Rook);
            Fields[0 + sevenRows] = new Figure(false, FigureType.Rook);

            Fields[1] = new Figure(true, FigureType.Knight);
            Fields[1 + sevenRows] = new Figure(false, FigureType.Knight);

            Fields[2] = new Figure(true, FigureType.Rook);
            Fields[2 + sevenRows] = new Figure(false, FigureType.Bishop);

            Fields[3] = new Figure(true, FigureType.Rook);
            Fields[3 + sevenRows] = new Figure(false, FigureType.Queen);

            Fields[4] = new Figure(true, FigureType.Rook);
            Fields[4 + sevenRows] = new Figure(false, FigureType.King);

            Fields[5] = new Figure(true, FigureType.Rook);
            Fields[5 + sevenRows] = new Figure(false, FigureType.Bishop);

            Fields[6] = new Figure(true, FigureType.Rook);
            Fields[6 + sevenRows] = new Figure(false, FigureType.Knight);

            Fields[7] = new Figure(true, FigureType.Rook);
            Fields[7 + sevenRows] = new Figure(false, FigureType.Rook);

            // Apply all mutations
            string[] movesSplitted = moves.Split(' ');
            IsWhiteToMove = movesSplitted.Length % 2 == 0;
            foreach (var move in movesSplitted)
                Mutate(move);
        }

        internal void Mutate(string move)
        {
            string from = move.Substring(0, 2);
            string to = move.Substring(2, 2);

            // Pick piece
            int fromIndex = GetIndex(from);
            Figure figureFrom = Fields[fromIndex];

            // remove from
            Fields[fromIndex].Type = FigureType.EMPTY;

            // Check destination
            int toIndex = GetIndex(to);
            Figure figureTo = Fields[toIndex];

            Fields[toIndex] = figureFrom;
        }

        public static int GetIndex(string from) => 8 * (from[0] - 'a') + (from[1] - '1');

        public static string GetUIC(int index) => new string(new char[] { (char)((index % 8) + 'a'), (char)((index / 8) + '1') });

        internal AllowedGameState GetBoardState()
        {
            throw new NotImplementedException();
        }

        internal List<string> GetLegalMoves()
        {
            List<string> moves = new List<string>();

            // Find each figure
            for (int i = 0; i < 8 * 8; i++)
            {
                Figure f = Fields[i];

                // XOR is same as saying that the color is not who is to move
                if (f.IsWhite ^ IsWhiteToMove)
                    continue;

                switch (f.Type)
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

            return moves;
        }

        private void AddPawnlikeMoves(List<string> moves, int i)
        {
            // if not on baseline anymore, can go one up if free
            int rowNum = i / 8;
            if (Fields[i].IsWhite)
            {
                // Can not jump anymore
                if (rowNum > 1)
                {
                    // can go straight
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

            // iterate over each
            foreach (var vec in vectors)
            {
                int index = i + vec.X + vec.Y * 8;
                AddIfEmptyOrEnemy(moves, index);
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
                    vectors.RemoveAll(x => x.X > 2);
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
                AddIfEmptyOrEnemy(moves, index);
            }
        }

        private readonly void AddIfEmptyOrEnemy(List<string> moves, int index)
        {
            if ((Fields[index].Type == FigureType.EMPTY)
                ||
                (Fields[index].IsWhite ^ IsWhiteToMove)
                )
            {
                moves.Add(GetUIC(index));
            }
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
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(index));

                    break;
                }

                moves.Add(GetUIC(index));
            }

            // Go left and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowsToTop); times++)
            {
                index += 8 - 1;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(index));

                    break;
                }

                moves.Add(GetUIC(index));
            }

            // Go left and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowNum); times++)
            {
                index += -8 - 1;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(index));

                    break;
                }

                moves.Add(GetUIC(index));
            }

            // Go right and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowNum); times++)
            {
                index += -8 + 1;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(index));

                    break;
                }

                moves.Add(GetUIC(index));
            }
        }

        private readonly void AddRooklikeMoves(List<string> moves, int i)
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
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(colI));

                    break;
                }

                moves.Add(GetUIC(colI));
            }

            // Go left until you hit something
            for (int colI = columnOffset - 1; colI >= 0; colI--)
            {
                int index = colI + rowOffset;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(colI));

                    break;
                }

                moves.Add(GetUIC(colI));
            }

            // Go up until you hit something
            for (int rowI = rowNum + 1; rowI < 8; rowI++)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(rowI));

                    break;
                }

                moves.Add(GetUIC(rowI));
            }

            // go down until you hit something
            for (int rowI = rowNum - 1; rowI >= 0; rowI--)
            {
                int index = columnOffset + rowI * 8;
                if (Fields[index].Type != FigureType.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        moves.Add(GetUIC(rowI));

                    break;
                }

                moves.Add(GetUIC(rowI));
            }
        }

        internal double Evaluate()
        {
            // simply count pieces
            double score = 0;
            for (int i = 0; i < 8 * 8; i++)
                score += Weighting(Fields[i].Type) * (Fields[i].IsWhite ? 1 : -1);

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

    public class Cache
    {
        public List<string> LegalMoves { get; set; }
        public double? Evaluation { get; set; }
    }
}