using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace TestCh
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBoardStatics()
        {
            // GetUIC
            Assert.AreEqual(Board.GetUCI(0, 1, MoveType.Move), "a1a2");
            Assert.AreEqual(Board.GetUCI(8 * 8 - 1, 7 * 8 - 1, MoveType.Move), "h8h7");
            Assert.AreEqual(Board.GetUCI(0, 1, MoveType.Take), "a1xa2");
            Assert.AreEqual(Board.GetUCI(8 * 8 - 1, 7 * 8 - 1, MoveType.Take), "h8xh7");

            // GetIndex
            Assert.AreEqual(Board.GetIndex("a1"), 0);
            Assert.AreEqual(Board.GetIndex("h8"), 8 * 8 - 1);
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

        [TestMethod]
        public void TestBestMove()
        {
            Engine eng = new Engine("d2d4");
            // most likely it's best to take!
            string reaction = eng.ReactToMove("d2d4 e7e5");
            Assert.AreEqual(reaction, "d4xe5");
        }
    }
}
