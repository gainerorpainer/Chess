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
            Assert.AreEqual(new UCINotation(new Move(0, 8, TypeOfMove.Move)).ToString(), "a1a2");
            Assert.AreEqual(new UCINotation(new Move(8 * 8 - 1, 7 * 8 - 1, TypeOfMove.Move)).ToString(), "h8h7");
            Assert.AreEqual(new UCINotation(new Move(0, 8, TypeOfMove.Take)).ToString(), "a1xa2");
            Assert.AreEqual(new UCINotation(new Move(8 * 8 - 1, 7 * 8 - 1, TypeOfMove.Take)).ToString(), "h8xh7");

            // GetIndex
            Assert.AreEqual(UCINotation.DeserializeMove("a1a2"), new Move(0, 8, TypeOfMove.Move));
            Assert.AreEqual(UCINotation.DeserializeMove("a1xb2"), new Move(0, 9, TypeOfMove.Take));
        }
    }
}