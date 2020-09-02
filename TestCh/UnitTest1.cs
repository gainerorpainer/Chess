using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCh
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBoardStatics()
        {
            // GetUIC
            Assert.AreEqual(Board.GetUIC(0), "a1");
            Assert.AreEqual(Board.GetUIC(8 * 8 - 1), "h8");

            // GetIndex
            Assert.AreEqual(Board.GetIndex("a1"), 0);
            Assert.AreEqual(Board.GetIndex("h8"), 8 * 8 - 1);
        }
    }
}
