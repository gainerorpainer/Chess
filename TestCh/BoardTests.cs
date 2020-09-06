using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace TestCh
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void TestValueSemantics()
        {
            Board original = new Board(Enumerable.Empty<Move>());
            Board copy = original;

            Assert.AreSame(original, copy);

            // make a value copy
            copy = (Board)original.Clone();

            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original, copy);

            // Test cache semantics
            copy.GetEvaluation();
            copy.GetBoardState();
            copy.GetLegalMoves();

            Assert.AreEqual(original, copy);
        }

        [TestMethod]
        public void TestStatics()
        {
            // Weighting
            Board freshBoard = new Board(Enumerable.Empty<Move>());
            double whiteEvaluation = freshBoard.Fields.Where(x => x.Figure != FigureType.EMPTY && x.IsWhite == true).Sum(x => Board.Weighting(x.Figure));
            double blackEvaluation = freshBoard.Fields.Where(x => x.Figure != FigureType.EMPTY && x.IsWhite == false).Sum(x => Board.Weighting(x.Figure));
            Assert.AreEqual(whiteEvaluation, blackEvaluation);
            Assert.AreEqual(whiteEvaluation, 8 * 1 + 1 * 9 + 2 * 5 + 2 * 3 + 2 * 3 + 50);
        }

        [TestMethod]
        public void TestAvailableMoves()
        {
            // in the first move, there must be the following moves available
            List<string> firstMoves = new List<string>()
            {
                "a2a3",
                "a2a4",
                "b2b3",
                "b2b4",
                "c2c3",
                "c2c4",
                "d2d3",
                "d2d4",
                "e2e3",
                "e2e4",
                "f2f3",
                "f2f4",
                "g2g3",
                "g2g4",
                "h2h3",
                "h2h4",

                "b1a3",
                "b1c3",

                "g1f3",
                "g1h3"
            };

            firstMoves.Sort();

            Board b = new Board(Enumerable.Empty<Move>());
            List<string> boardMoves = b.GetLegalMoves().Select(x => new UCINotation(x).ToString()).ToList();

            boardMoves.Sort();

            var t = firstMoves.Where(x => boardMoves.Contains(x) == false).ToList();

            CollectionAssert.AreEquivalent(firstMoves, boardMoves);
        }


    }
}
