﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace TestCh
{
    [TestClass]
    public class CodeGen
    {
        [TestMethod]
        public void GenKnightMoves()
        {
            // There are 8 configurations
            List<Point> baseVector = new List<Point>() {
                new Point(1, 2),
                new Point(2, 1),
                new Point(2, -1),
                new Point(1, -2),
                new Point(-1, -2),
                new Point(-2, -1),
                new Point(-2, 1),
                new Point(-1, 2),
            };

            // Result is a vector which associates up to 8 possible moves to a board location. 
            int[,] result = new int[8 * 8, 8];

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
                    // -1 is end of collection by convention
                    for (int i = 0; i < 8; i++)
                    {
                        int index = 8 * rowNumber + colNumber;
                        result[index, i] =
                            i < vectors.Count ?
                                vectors[i].X + (vectors[i].Y * 8)
                            :
                                -1;

                    }
                }

            }

            // Serialize result

            string arr = "public static readonly int[,] KNIGHT_LOOKUP = CreateKnightLookup();";
            arr += Environment.NewLine;
            arr += "private static int[,] CreateKnightLookup() " + Environment.NewLine + "{" + Environment.NewLine;
            arr += "var result = new int[8*8,8];";
            for (int i = 0; i < 8 * 8; i++)
            {
                arr += $"result[{i}] = new int[] {{ ";
                arr += string.Join(", ", Enumerable.Range(0, 8).Select(x => result[i, x]));
                arr += " };" + Environment.NewLine;
            }


            arr += "return result;";
            arr += '}';
        }
    }
}