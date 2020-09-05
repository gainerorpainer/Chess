using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChEngine.Tests
{
    [TestClass()]
    public class EngineTests
    {
        [TestMethod()]
        public void TestBestMove()
        {
            Engine eng = new Engine("d2d4");
            // most likely it's best to take!
            string reaction = eng.ReactToMove("d2d4 e7e5");
            Assert.AreEqual(reaction, "d4xe5");
        }
    }
}