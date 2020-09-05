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
            Board original = new Board("");
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
            // GetUIC
            Assert.AreEqual(Board.GetUCI(0, 8, MoveType.Move), "a1a2");
            Assert.AreEqual(Board.GetUCI(8 * 8 - 1, 7 * 8 - 1, MoveType.Move), "h8h7");
            Assert.AreEqual(Board.GetUCI(0, 8, MoveType.Take), "a1xa2");
            Assert.AreEqual(Board.GetUCI(8 * 8 - 1, 7 * 8 - 1, MoveType.Take), "h8xh7");

            // GetIndex
            Assert.AreEqual(Board.ParseMove("a1a2"), new Move(0, 8, MoveType.Move));
            Assert.AreEqual(Board.ParseMove("a1xb2"), new Move(0, 9, MoveType.Take));

            // Weighting
            Board freshBoard = new Board("");
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

            Board b = new Board("");
            List<string> boardMoves = b.GetLegalMoves();

            boardMoves.Sort();

            var t = firstMoves.Where(x => boardMoves.Contains(x) == false).ToList();

            CollectionAssert.AreEquivalent(firstMoves, boardMoves);
        }


    }
}
