using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCh
{
    [TestClass]
    public class EvaluationTests
    {
        [TestMethod]
        public void TestGetEvaluation()
        {
            // Weighting
            Board freshBoard = new Board(Enumerable.Empty<Move>());
            double whiteEvaluation = freshBoard.Fields.Where(x => x.Figure != TypeOfFigure.EMPTY && x.IsWhite == true).Sum(x => Evaluation.Weighting(x.Figure));
            double blackEvaluation = freshBoard.Fields.Where(x => x.Figure != TypeOfFigure.EMPTY && x.IsWhite == false).Sum(x => Evaluation.Weighting(x.Figure));
            Assert.AreEqual(whiteEvaluation, blackEvaluation);
            Assert.AreEqual(whiteEvaluation, 8 * 1 + 1 * 9 + 2 * 5 + 2 * 3 + 2 * 3 + 50);
            Assert.AreEqual(0, Evaluation.GetEvaluation(freshBoard));
        }
    }
}
