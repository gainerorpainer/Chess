using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

            // make sure a copy was made
            Assert.AreNotSame(original, copy);
            Assert.AreNotSame(original.Fields[0], copy.Fields[0]);
            Assert.AreNotSame(original.PlayerOptions[0], copy.PlayerOptions[0]);

            // assert value semantics
            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.Fields[0], copy.Fields[0]);
            Assert.AreEqual(original.PlayerOptions[0], copy.PlayerOptions[0]);

            // mutate the copy and check that nothing changed on the original
            var originalFieldBefore = original.Fields[UCINotation.UCIToIndex("a2")];
            copy.Mutate(new Move(UCINotation.UCIToIndex("a2"), UCINotation.UCIToIndex("a3"), TypeOfMove.Move));
            var originalFieldAfter = original.Fields[UCINotation.UCIToIndex("a2")];
            //Assert.AreSame(originalFieldBefore, originalFieldAfter);
            Assert.IsTrue(originalFieldBefore.GetType().IsValueType);
            Assert.AreEqual(originalFieldBefore, originalFieldAfter);

            // redo copy
            copy = (Board)original.Clone();

            // Test cache semantics
            copy.GetEvaluation();
            copy.GetLegalMoves();

            Assert.AreEqual(original, copy);
        }

        [TestMethod]
        public void TestStatics()
        {
            // Weighting
            Board freshBoard = new Board(Enumerable.Empty<Move>());
            double whiteEvaluation = freshBoard.Fields.Where(x => x.Figure != TypeOfFigure.EMPTY && x.IsWhite == true).Sum(x => Board.Weighting(x.Figure));
            double blackEvaluation = freshBoard.Fields.Where(x => x.Figure != TypeOfFigure.EMPTY && x.IsWhite == false).Sum(x => Board.Weighting(x.Figure));
            Assert.AreEqual(whiteEvaluation, blackEvaluation);
            Assert.AreEqual(whiteEvaluation, 8 * 1 + 1 * 9 + 2 * 5 + 2 * 3 + 2 * 3 + 50);
        }

        [TestMethod]
        public void TestStartPositionMoves()
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

        [TestMethod]
        public void Test_PinnedPawn()
        {
            Board b = new Board(new string[]
            {
                "e2e4", // Expand white king pawn
                "d7d5", // offer black queen pawn
                "d1e2", // queen oposite black king
                "e7e6", // expand king pawn
                "e4d5", // take queen pawn, pin black king pawn
            }.Select(x => UCINotation.DeserializeMove(x)));

            // the pawn on d6 cannot take
            Move illegalMove = UCINotation.DeserializeMove("e6xd5");
            CollectionAssert.Contains(b.GetMoves_IgnoreCheckRules().ToList(), illegalMove);
            CollectionAssert.DoesNotContain(b.GetLegalMoves(), illegalMove);
        }

        [TestMethod]
        public void Test_StartInCheck()
        {
            Board b = new Board(new string[]
            {
                "f2f4", // open up king diagnonal
                "d7d5", // open up queen diag
                "f4d5", // does not really matter
                "d8h4", // queen check white king
            }.Select(x => UCINotation.DeserializeMove(x)));

            // many king ignoring moves
            Assert.IsTrue(b.GetMoves_IgnoreCheckRules().Count() > 1);

            // Only legal move!
            var moves = b.GetLegalMoves();

            Assert.IsTrue(moves.Count == 1);
            Assert.AreEqual(moves.First(), UCINotation.DeserializeMove("g2g3"));
        }

        [TestMethod]
        public void Test_Castling()
        {
            Board b = new Board(new string[]
            {
                "e2e3", // open up bishop diagnal
                "a7a6", // anything
                "f1e2", // move bishop
                "a6a5", // anything
                "g1f3", // move knight
                "a5a4", // anything
            }.Select(x => UCINotation.DeserializeMove(x)));

            // castle kingside
            var castleKingside = UCINotation.DeserializeMove("e1g1");
            var moves = b.GetLegalMoves();

            CollectionAssert.Contains(moves, castleKingside);
        }

        [TestMethod]
        public void Test_Enpassant()
        {
            Board b = new Board(new string[]
            {
                "e2e4", // Advance
                "a7a6", // anything
                "e4e5", // Advance further
                "f7f5", // longjump
            }.Select(x => UCINotation.DeserializeMove(x)));

            Assert.IsTrue(b.PlayerOptions[b.CurrentPlayerId()].CheckEnpassantOnCol(5));

            var enpassantTakes = UCINotation.DeserializeMove("e5xf6");
            var moves = b.GetLegalMoves();

            CollectionAssert.Contains(moves, enpassantTakes);

            // make another move and lose the option to enpassant
            b.Mutate(UCINotation.DeserializeMove("a2a3"));
            Assert.IsFalse(b.PlayerOptions[b.OtherPlayerId()].CheckEnpassantOnCol(5));

            // which cannot be made next move either
            b.Mutate(UCINotation.DeserializeMove("a5a4"));
            moves = b.GetLegalMoves();
            CollectionAssert.DoesNotContain(moves, enpassantTakes);
        }
    }
}
