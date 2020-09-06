using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine.Tests
{
    [TestClass()]
    public class UCINotationTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            // GetUIC
            Assert.AreEqual(new UCINotation(new Move(0, 8, MoveType.Move)).ToString(), "a1a2");
            Assert.AreEqual(new UCINotation(new Move(8 * 8 - 1, 7 * 8 - 1, MoveType.Move)).ToString(), "h8h7");
            Assert.AreEqual(new UCINotation(new Move(0, 8, MoveType.Take)).ToString(), "a1xa2");
            Assert.AreEqual(new UCINotation(new Move(8 * 8 - 1, 7 * 8 - 1, MoveType.Take)).ToString(), "h8xh7");

            // GetIndex
            Assert.AreEqual(UCINotation.ParseMove("a1a2"), new Move(0, 8, MoveType.Move));
            Assert.AreEqual(UCINotation.ParseMove("a1xb2"), new Move(0, 9, MoveType.Take));
        }
    }
}