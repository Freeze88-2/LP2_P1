using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    internal class ZetaAI : AbstractThinker
    {
        private readonly float winScore = 100000;
        private int maxDepth;
        private IEnumerable<Pos>[] positions;
        private bool firsttime;
        private readonly ZetaHeuristic heuristic;
        private Stopwatch timer;
        private Stopwatch functionTime;

        /// <summary>
        /// Constructor of the ZetaAI class
        /// </summary>
        public ZetaAI()
        {
            heuristic = new ZetaHeuristic();
            firsttime = true;
            timer = new Stopwatch();
            functionTime = new Stopwatch();
        }

        /// <summary>
        /// Allows the AI to display versions when called ToString
        /// </summary>
        /// <returns> The new name of the AI </returns>
        public override string ToString()
        {
            return "ZetaAI_pack2";
        }

        /// <summary>
        /// The Think method (mandatory override) is invoked by the game engine
        /// </summary>
        /// <param name="board"> The current state of the board </param>
        /// <param name="ct"> A cancelletion token </param>
        /// <returns></returns>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            // Starts the timer for the whole AI
            timer.Restart();

            // Sets the maximum depth to one
            maxDepth = 1;

            // If it's the first time converts the wincorridors to array
            if (firsttime)
            {
                positions = board.winCorridors.ToArray();
                firsttime = false;
            }

            // Starts the timer for running the function at one depth
            functionTime.Restart();

            (FutureMove move, float value) b = NegaMax(board.Copy(), board.Turn
                , 0, float.NegativeInfinity, float.PositiveInfinity, ct);

            // Stops the timer after the function ends
            functionTime.Stop();

            // Resets the maximum depth to 0
            maxDepth = 0;

            // Number of boards one cycle takes
            int boardCount = board.cols * 2;

            // Cheks if the AI still has time
            while (timer.ElapsedMilliseconds + (Math.Pow(boardCount, maxDepth
                + 1) * (functionTime.ElapsedMilliseconds / Math.Pow(boardCount,
                maxDepth + 1))) < TimeLimitMillis * 0.2f)
            {
                // Increments the depth by one
                maxDepth++;

                // Calss the NegaMax method returning a furture move and value
                b = NegaMax(board.Copy(), board.Turn, 0, float.NegativeInfinity
                    , float.PositiveInfinity, ct);

                if (b.value == winScore)
                    break;
            }
            // Stops the timer
            timer.Stop();

            // Returns the intended move
            return b.move;
        }

        /// <summary>
        /// NegaMax method responsible for finding the best move
        /// </summary>
        /// <param name="board"> The current board </param>
        /// <param name="turn"> Who's turn it is </param>
        /// <param name="depth"> The current depth </param>
        /// <param name="alpha"> Alpha value </param>
        /// <param name="beta"> Beta value </param>
        /// <param name="ct"> Cancellation token </param>
        /// <returns> A future move and a value </returns>
        private (FutureMove move, float value) NegaMax(Board board, PColor turn
            , int depth, float alpha, float beta, CancellationToken ct)
        {
            // Checks if a cancelletion was called
            if (ct.IsCancellationRequested)
            {
                return (FutureMove.NoMove, 0);
            }

            // Checks if there's a winner
            if (board.CheckWinner() != Winner.None)
            {
                // If the winner is the current player
                if (board.CheckWinner() == turn.ToWinner())
                {
                    // Returns the maximum socre
                    return (FutureMove.NoMove, winScore);
                }
                else
                {
                    // Returns the minimum score
                    return (FutureMove.NoMove, -winScore);
                }
            }
            // Checks if the current depth is the maximum
            else if (depth == maxDepth)
            {
                // Finds the heuristic value of the board and returns it
                return (FutureMove.NoMove, heuristic.HeuristicValue(board,
                    turn, positions));
            }
            // If none of the above
            else
            {
                // Creates a variable with a future move and value
                (FutureMove move, float value) bestMove =
                    (FutureMove.NoMove, float.NegativeInfinity);

                // Runs a cycle for each collumn in the board
                for (int colsN = 0; colsN < board.cols; colsN++)
                {
                    // If the collumn is full it skips the rest
                    if (board.IsColumnFull(colsN))
                    {
                        continue;
                    }

                    // A loop for checking both square and circle pieces
                    for (int shapeN = 0; shapeN < 2; shapeN++)
                    {
                        // Checks if the current player has pieces of the type
                        if (board.PieceCount(turn, (PShape)shapeN) == 0)
                        {
                            continue;
                        }

                        // Creates a new FutureMove with the current collumn
                        // and the PShape type acording to b
                        FutureMove pos = new FutureMove(colsN, (PShape)shapeN);

                        // Does a move on the board with the pos values
                        board.DoMove(pos.shape, pos.column);

                        // Creates a score variable and assigns it the value
                        // given by an iteration of the NegaMax method
                        float score = -NegaMax(board, turn.Other(), depth + 1,
                            -beta, -alpha, ct).value;

                        // Undos the move done
                        board.UndoMove();

                        // Checks if the score of a board is bigger than alpha
                        if (score > alpha)
                        {
                            // Assigns the value of aplha the value of score
                            alpha = score;
                            // Assigns the bestMove the value of pos and alpha
                            bestMove = (pos, alpha);

                            // Checks if there's Alpha Beta cuts
                            if (alpha >= beta)
                            {
                                // If there is returns the current bestMove
                                return bestMove;
                            }
                        }
                    }
                }
                // If all else fails returns the current bestMove
                return bestMove;
            }
        }
    }
}