using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace ChEngine
{
    public class Engine
    {
        public event EventHandler<string> MoveDecided;
        public bool IsWhite { get; set; }

        private CancellationTokenSource cancellationTokenSource;

        private readonly object mutex = new object();

        public Engine(string beginMoves)
        {
            IsWhite = beginMoves.Length == 0;
        }

        public string ReactToMove(string moves)
        {
            var newBoard = new Board(moves);

            double bestScore = double.MaxValue + -1;
            string bestMove;

            // Make a move within the next 3 seconds
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(3000);

            List<string> myMoves = newBoard.GetLegalMoves();
            // There must be at least one move, otherwise this would already be checkmate!
            bestMove = myMoves.First();

            foreach (var myMove in myMoves)
            {
                double score = EvaluateContinuation(newBoard, myMove);

                lock (mutex)
                {
                    if (Sign(score) > bestScore)
                    {
                        bestScore = Sign(score);
                        bestMove = myMove;
                    }
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Evaluates recursively one continuation (of me)
        /// </summary>
        /// <param name="board">Board to start from</param>
        /// <param name="move">Move to apply</param>
        /// <returns>Number that is better the higher it is</returns>
        private double EvaluateContinuation(Board board, string move)
        {
            board.Mutate(move);

            // check each legal move for enemy 
            foreach (var enemyMove in board.GetLegalMoves())
            {
                Board copy = board;
                copy.Mutate(enemyMove);

                var legalMoves = copy.GetLegalMoves();

                if (legalMoves.Count == 0)
                {
                    if (copy.GetBoardState() == AllowedGameState.Checkmate)
                        // Enemy checkmates me
                        return double.MaxValue * -1;
                    else
                        throw new NotImplementedException();
                }

                // if there is no more time left, just evaluate and break the recursion
                if (cancellationTokenSource.IsCancellationRequested)
                    return copy.Evaluate();

                // else go in recursion
                // this continuation is only as good as the worst recursive evalúation
                double worstScore = double.MaxValue;
                foreach (var myMove in legalMoves)
                {
                    double score = EvaluateContinuation(copy, myMove);
                    if (Sign(score) < worstScore)
                        worstScore = Sign(score);
                }

                return Sign(worstScore);
            }

            // if enemy has no legal moves, this results in checkmate
            return double.MaxValue;
        }

        /// <summary>
        /// Applies a sign such that the score is flipped for black
        /// </summary>
        /// <param name="score">Input score</param>
        /// <returns>score or -score if black</returns>
        private double Sign(double score) => IsWhite ? score : -score;
    }
}
