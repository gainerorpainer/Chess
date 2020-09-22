﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ChEngine
{
    public class Engine
    {
        public bool IsWhite { get; set; }
        public int MaxDepth { get; set; } = 3;

        private const double BEST_SCORE = double.MaxValue;
        private const double WORST_SCORE = -BEST_SCORE;
        private CancellationTokenSource cancellationTokenSource;

        public Engine(bool isWhite)
        {
            IsWhite = isWhite;
        }

        public Move ReactToMove(IEnumerable<Move> moves)
        {
            // Make a move within the next 3 seconds
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(60000);
            //cancellationTokenSource.Cancel();

            var initialBoard = new Board(moves);

            // Reset stats
            InterlockedEngineStats.Reset();

            // Start with the first tree layer
            List<TreeNode> lastTreeLayer = initialBoard.GetLegalMoves().Select(x => new TreeNode()
            {
                Board = new Board(moves.Append(x)), // This is far from optimal, but ok since this is only done in init phase
                Children = new List<TreeNode>(),
                Evaluation = double.NaN,
                Parent = null,
                Move = x
            }).ToList();

            // Start building a tree
            List<TreeNode> gameTree = new List<TreeNode>(lastTreeLayer);
            int depth = 0;

            do
            {
                List<TreeNode> newTreeLayer = new List<TreeNode>();

                foreach (var parentNode in lastTreeLayer)
                {
                    foreach (var possibleMove in parentNode.Board.GetLegalMoves())
                    {
                        // make a copy and change it
                        Board branchBoardAfterMove = (Board)(parentNode.Board.Clone());
                        branchBoardAfterMove.Mutate(possibleMove);

                        TreeNode childNode = new TreeNode()
                        {
                            Board = branchBoardAfterMove,
                            Children = new List<TreeNode>(),
                            Evaluation = double.NaN,
                            Parent = parentNode,
                            IsComplete = false,
                            Move = possibleMove
                        };

                        newTreeLayer.Add(childNode);
                        parentNode.Children.Add(childNode);
                    }

                    parentNode.IsComplete = true;

                    // Timeout applied
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                }

                if (cancellationTokenSource.IsCancellationRequested)
                    break;

                InterlockedEngineStats.Update_MaxDepth(++depth);

                // swap
                lastTreeLayer = newTreeLayer;

                if (depth >= MaxDepth)
                    break;

            } while (!cancellationTokenSource.IsCancellationRequested);

            // Traverse tree
            double bestScore = Sign(WORST_SCORE);
            Move bestMove = gameTree.First().Move;
            foreach (var candidate in gameTree)
            {
                //if (UCINotation.SerializeMove(candidate.Move) == "a3b5")
                //{
                //    sw.Close();
                //    sw = new StreamWriter("test.txt");
                //}

                double deepEvaluation = RecursiveEvaluateNode(candidate);
                if (Sign(deepEvaluation) > Sign(bestScore))
                {
                    bestScore = deepEvaluation;
                    bestMove = candidate.Move;
                }


                //if (UCINotation.SerializeMove(candidate.Move) == "a3b5")
                //{
                //    sw.Flush();
                //    sw.Close();

                //    // turn file upside down
                //    File.WriteAllLines("test.txt", File.ReadAllLines("test.txt").Reverse());
                //}
            }

            sw.Flush();
            sw.Close();

            // turn file upside down
            File.WriteAllLines("test.txt", File.ReadAllLines("test.txt").Reverse());

            InterlockedEngineStats.Set_Evaluation(bestScore);

            return bestMove;
        }


        StreamWriter sw = new StreamWriter("test.txt");

        /// <summary>
        /// Recursively searches a tree node and gives the evaluation for this node assuming best play for each player
        /// </summary>
        /// <param name="node">Current node</param>
        /// <returns>Evaluation (pos: white advantage)</returns>
        private double RecursiveEvaluateNode(TreeNode node, int depth = 0)
        {

            InterlockedEngineStats.Increment_NodesVisited();

            // End of recursion
            if (node.Children.Count == 0)
            {
                double eval = Evaluation.GetEvaluation(node.Board.Fields);

                for (int i = 0; i < depth; i++)
                    sw.Write('\t');
                sw.Write(node.Board.IsWhiteToMove ? "w: " : "b: ");
                sw.WriteLine(UCINotation.SerializeMove(node.Move) + " -> " + eval);

                return eval;
            }

            // Assume that the enemy (child node) will make a best move for him
            double enemySign = GetSign(!node.Board.IsWhiteToMove);
            double bestEnemyMove = enemySign * WORST_SCORE;

            foreach (var childNode in node.Children)
            {
                double enemyScore = RecursiveEvaluateNode(childNode, depth + 1);
                if ((enemySign * enemyScore) > (enemySign * bestEnemyMove))
                    bestEnemyMove = enemyScore;
            }

            for (int i = 0; i < depth; i++)
                sw.Write('\t');
            sw.Write(node.Board.IsWhiteToMove ? "w: " : "b: ");
            sw.WriteLine(UCINotation.SerializeMove(node.Move) + " -> " + bestEnemyMove);

            return bestEnemyMove;
        }

        /// <summary>
        /// Get a signed 1.0 depending on the color
        /// </summary>
        /// <param name="isWhite"></param>
        /// <returns>+1.0 if white, -1.0 if not white</returns>
        static double GetSign(bool isWhite) => isWhite ? 1 : -1;

        /// <summary>
        /// Applies a sign such that the score is flipped for black
        /// </summary>
        /// <param name="score">Input score</param>
        /// <returns>score or -score if black</returns>
        private double Sign(double score) => IsWhite ? score : -score;
    }
}
