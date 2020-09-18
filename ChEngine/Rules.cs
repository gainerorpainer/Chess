using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ChEngine
{
    public static class Rules
    {
        // Defines all possible knight moves based on the location
        public static readonly int[][] LOOKUP_KNIGHTMOVES = CreateKnightLookup();

        // Defines all possible king moves based on the location
        public static readonly int[][] LOOKUP_KINGMOVES = CreateKingLookup();

        // Get the default field for standard chess
        public static readonly Field[] DEFAULT_FIELDS = CreateDefaultField();

        // Get the default player options
        public static readonly PlayerOption[] DEFAULT_PLAYER_OPTIONS = new PlayerOption[] { PlayerOption.DefautOption, PlayerOption.DefautOption };


        private static int[][] CreateKingLookup()
        {
            // There are 8 configurations
            List<Point> baseVector = new List<Point>() {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(1, -1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(-1, 0),
                new Point(-1, 1),
            };


            // Result is a vector which associates up to 8 possible moves to a board location. 
            int[][] result = new int[8 * 8][];


            for (int rowNumber = 0; rowNumber < 8; rowNumber++)
            {
                for (int colNumber = 0; colNumber < 8; colNumber++)
                {
                    var vectors = new List<Point>(baseVector);

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


                    // copy to result
                    int index = colNumber + 8 * rowNumber;
                    result[index] = vectors.Select(x => index + x.X + 8 * x.Y).ToArray();
                }
            }

            return result;
        }

        private static int[][] CreateKnightLookup()
        {
            // There are 8 configurations
            List<Point> baseVector = new List<Point>() {
                new Point(-2, -1),
                new Point(-2, 1),
                new Point(-1, -2),
                new Point(-1, 2),
                new Point(1, -2),
                new Point(1, 2),
                new Point(2, -1),
                new Point(2, 1),
            };

            // Result is a vector which associates up to 8 possible moves to a board location. 
            int[][] result = new int[8 * 8][];

            for (int rowNumber = 0; rowNumber < 8; rowNumber++)
            {
                for (int colNumber = 0; colNumber < 8; colNumber++)
                {
                    var vectors = new List<Point>(baseVector);

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

                    // copy to result
                    int index = colNumber + 8 * rowNumber;
                    result[index] = vectors.Select(x => index + x.X + 8 * x.Y).ToArray();
                }

            }

            return result;
        }

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


    }
}
