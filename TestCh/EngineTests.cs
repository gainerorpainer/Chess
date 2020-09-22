using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChEngine.Tests
{
    [TestClass()]
    public class EngineTests
    {
        [TestMethod()]
        public void TestBestMove()
        {
            Engine eng = new Engine(isWhite: true);
            // most likely it's best to take!
            Move reaction = eng.ReactToMove(new string[] { "d2d4", "e7e5" }.Select(x => UCINotation.DeserializeMove(x)));
            Assert.AreEqual(reaction, UCINotation.DeserializeMove("d4xe5"));
        }

        [TestMethod]
        public void TestBestMove2()
        {
            Engine eng = new Engine(isWhite: true);
            // most likely it's best to take!
            Move reaction = eng.ReactToMove(new string[] { "b1a3", "d7d5", "a1a2", "b7b5" }.Select(x => UCINotation.DeserializeMove(x)));
            Assert.AreEqual(reaction, UCINotation.DeserializeMove("a3xb5"));
        }
    }
}