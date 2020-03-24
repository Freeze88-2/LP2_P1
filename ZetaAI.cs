using System;
using System.Threading;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    internal class ZetaAI : AbstractThinker
    {
        private readonly float winScore = 100000;
        private int maxDepth;

        /// <summary>
        /// The Setup() method, optional override
        /// </summary>
        /// <param name="str"> A string of the argument passed </param>
        public override void Setup(string str)
        {
            // Try to get the maximum depth from the parameters
            if (!int.TryParse(str, out maxDepth))
            {
                // If not possible, set it to 3 by default
                maxDepth = 4;
            }
        }

        /// <summary>
        /// The Think method (mandatory override) is invoked by the game engine
        /// </summary>
        /// <param name="board"> The current state of the board </param>
        /// <param name="ct"> A cancelletion token </param>
        /// <returns></returns>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            // Invokes the NegaMax method returning a value and a furture move
            return NegaMax(board.Copy(), board.Turn, 0, float.NegativeInfinity,
                float.PositiveInfinity, ct).move;
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
            if (ct.IsCancellationRequested) return (FutureMove.NoMove, 0);

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
                    // Returns the minimum score
                    return (FutureMove.NoMove, -winScore);
            }
            // Checks if the current depth is the maximum
            else if (depth == maxDepth)
            {
                // Finds the heuristic value of the board and returns it
                return (FutureMove.NoMove, HeuristicValue(board, turn));
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
                    if (board.IsColumnFull(colsN)) continue;

                    // A loop for checking both square and circle pieces
                    for (int shapeN = 0; shapeN < 2; shapeN++)
                    {
                        // Checks if the current player has pieces of the type
                        if (board.PieceCount(turn, (PShape)shapeN) == 0)
                            continue;

                        // Creates a new FutureMove with the current collumn
                        // and the PShape type acording to b
                        FutureMove pos = new FutureMove(colsN, (PShape)shapeN);

                        // Does a move on the board with the pos values
                        board.DoMove(pos.shape, pos.column);

                        // Creates a score variable and assigns it the value
                        // given by an iteration of the NegaMax method
                        float score = -NegaMax(board.Copy(), turn.Other(), depth + 1, -beta, -alpha, ct).value;

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
                                // If there is returns the current bestMove
                                return bestMove;
                        }
                    }
                }
                // If all else fails returns the current bestMove
                return bestMove;
            }
        }

        private float HeuristicValue(Board board, PColor turn)
        {
            float boardValue = 0;
            int nOfCollums = 0;

            float centerRow = board.rows / 2;
            float centerCol = board.cols / 2;

            float maxPoints = Dist(centerRow, centerCol, 0, 0);

            for (int j = 0; j < board.cols; j++)
            {
                for (int f = 0; f < board.rows; f++)
                {
                    if (board[f, j].HasValue)
                    {
                        Piece? piece = board[f, j];

                        if (turn.FriendOf(piece.Value))
                            boardValue += maxPoints - Dist(centerRow, centerCol, j, f);
                        else
                            boardValue -= maxPoints - Dist(centerRow, centerCol, j, f);

                        // Both sides
                        if (j - 1 >= 0 && j + 1 <= board.cols - 1)
                        {
                            Piece? emptySpace = piece;
                            Piece? emptySpace2 = piece;

                            if (j + 2 <= board.cols - 1)
                            {
                                emptySpace = board[f, j + 2];
                            }
                            if (j - 2 >= 0)
                            {
                                emptySpace2 = board[f, j - 2];
                            }

                            if (board[f, j - 1].HasValue && board[f, j + 1].HasValue && (!emptySpace.HasValue || !emptySpace2.HasValue))
                            {
                                if ((board[f, j - 1].Value.color.FriendOf(piece.Value) && board[f, j + 1].Value.color.FriendOf(piece.Value)))
                                {
                                    int boardExtra = 100;
                                    int columsLess = 1;

                                    if (!turn.FriendOf(piece.Value))
                                    {
                                        boardExtra = -boardExtra;
                                        columsLess = -columsLess;
                                    }

                                    if (!emptySpace.HasValue && !emptySpace2.HasValue)
                                    {
                                        boardValue += boardExtra;
                                        nOfCollums += columsLess;
                                    }
                                    else if (!emptySpace.HasValue || !emptySpace2.HasValue)
                                    {
                                        boardValue += boardExtra * 2;
                                        nOfCollums += columsLess;
                                    }
                                }
                            }
                            else
                            {
                                if (board[f, j - 1].HasValue)
                                {
                                    if (board[f, j - 1].Value.color.FriendOf(piece.Value))
                                    {
                                        if (turn.FriendOf(piece.Value))
                                            boardValue += 2;
                                        else
                                            boardValue -= 2;
                                    }
                                }
                                if (board[f, j + 1].HasValue)
                                {
                                    if (board[f, j + 1].Value.color.FriendOf(piece.Value))
                                    {
                                        if (turn.FriendOf(piece.Value))
                                            boardValue += 2;
                                        else
                                            boardValue -= 2;
                                    }
                                }
                            }
                        }

                        // Diagonals right
                        if (f - 2 >= 0 && j + 2 <= board.cols - 1)
                        {
                            Piece? emptySpace = piece;
                            Piece? emptySpace2 = piece;

                            if (f - 3 >= 0 && j + 3 <= board.cols - 1)
                            {
                                emptySpace = board[f - 3, j + 3];
                            }
                            if (f + 1 <= board.rows - 1 && j - 1 >= 0)
                            {
                                emptySpace2 = board[f + 1, j - 1];
                            }

                            if (board[f - 2, j + 2].HasValue && board[f - 1, j + 1].HasValue && (!emptySpace.HasValue || !emptySpace2.HasValue))
                            {
                                if ((board[f - 1, j + 1].Value.color.FriendOf(piece.Value) && board[f - 2, j + 2].Value.color.FriendOf(piece.Value)))
                                {
                                    if (piece.Value.shape == turn.Shape() || piece.Value.color == turn)
                                    {
                                        int boardExtra = 100;
                                        int columsLess = 1;

                                        if (!turn.FriendOf(piece.Value))
                                        {
                                            boardExtra = -boardExtra;
                                            columsLess = -columsLess;
                                        }

                                        if (!emptySpace.HasValue && !emptySpace2.HasValue)
                                        {
                                            boardValue += boardExtra;
                                            nOfCollums += columsLess;
                                        }
                                        else if (!emptySpace.HasValue || !emptySpace2.HasValue)
                                        {
                                            boardValue += boardExtra * 2;
                                            nOfCollums += columsLess;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (board[f - 1, j + 1].HasValue)
                                {
                                    if (board[f - 1, j + 1].Value.color.FriendOf(piece.Value))
                                    {
                                        if (turn.FriendOf(piece.Value))
                                            boardValue += 2;
                                        else
                                            boardValue -= 2;
                                    }
                                }
                                if (f + 1 <= board.rows - 1 && j - 1 >= 0)
                                {
                                    if (board[f + 1, j - 1].HasValue)
                                    {
                                        if (board[f + 1, j - 1].Value.color.FriendOf(piece.Value))
                                        {
                                            if (turn.FriendOf(piece.Value))
                                                boardValue += 2;
                                            else
                                                boardValue -= 2;
                                        }
                                    }
                                }
                            }
                        }

                        // Diagonals left
                        if (f - 2 >= 0 && j - 1 >= 0)
                        {
                            Piece? emptySpace = piece;
                            Piece? emptySpace2 = piece;

                            if (f - 3 >= 0 && j - 3 >= 0)
                            {
                                emptySpace = board[f - 3, j - 3];
                            }
                            if (f + 1 <= board.rows - 1 && j + 1 <= board.cols - 1)
                            {
                                emptySpace2 = board[f + 1, j + 1];
                            }

                            if (f - 2 >= 0 && j - 2 >= 0)
                            {
                                if (board[f - 2, j - 2].HasValue && board[f - 1, j - 1].HasValue && (!emptySpace.HasValue || !emptySpace2.HasValue))
                                {
                                    if ((board[f - 1, j - 1].Value.color.FriendOf(piece.Value) && board[f - 2, j - 2].Value.color.FriendOf(piece.Value)))
                                    {
                                        int boardExtra = 100;
                                        int columsLess = 1;

                                        if (!turn.FriendOf(piece.Value))
                                        {
                                            boardExtra = -boardExtra;
                                            columsLess = -columsLess;
                                        }

                                        if (!emptySpace.HasValue && !emptySpace2.HasValue)
                                        {
                                            boardValue += boardExtra;
                                            nOfCollums += columsLess;
                                        }
                                        else if (!emptySpace.HasValue || !emptySpace2.HasValue)
                                        {
                                            boardValue += boardExtra * 2;
                                            nOfCollums += columsLess;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (board[f - 1, j - 1].HasValue)
                                {
                                    if (board[f - 1, j - 1].Value.color.FriendOf(piece.Value))
                                    {
                                        if (turn.FriendOf(piece.Value))
                                            boardValue += 2;
                                        else
                                            boardValue -= 2;
                                    }
                                }
                                if (f + 1 <= board.rows - 1 && j + 1 <= board.cols - 1)
                                {
                                    if (board[f + 1, j + 1].HasValue)
                                    {
                                        if (board[f + 1, j + 1].Value.color.FriendOf(piece.Value))
                                        {
                                            if (turn.FriendOf(piece.Value))
                                                boardValue += 2;
                                            else
                                                boardValue -= 2;
                                        }
                                    }
                                }
                            }
                        }

                        // Down
                        if (f - 1 >= 0 && f - 2 >= 0)
                        {
                            Piece? emptySpace = piece;

                            if (f + 1 < board.rows - 1)
                            {
                                emptySpace = board[f + 1, j];
                            }

                            if (board[f - 1, j].HasValue && board[f - 2, j].HasValue && f + 1 <= board.rows - 1)
                            {
                                if ((board[f - 1, j].Value.color.FriendOf(piece.Value) && board[f - 2, j].Value.color.FriendOf(piece.Value)))
                                {
                                    int boardExtra = 100;
                                    int columsLess = 1;

                                    if (!turn.FriendOf(piece.Value))
                                    {
                                        boardExtra = -boardExtra;
                                        columsLess = -columsLess;
                                    }

                                    if (!emptySpace.HasValue)
                                    {
                                        boardValue += boardExtra;
                                        nOfCollums += columsLess;
                                    }
                                }
                            }
                            else
                            {
                                if (board[f - 1, j].HasValue)
                                {
                                    if (board[f - 1, j].Value.color.FriendOf(piece.Value))
                                    {
                                        if (turn.FriendOf(piece.Value))
                                            boardValue += 2;
                                        else
                                            boardValue -= 2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (nOfCollums > 1)
                boardValue += 1000;
            else if (nOfCollums < -1)
                boardValue -= 1000;
            return (boardValue * 100);
        }

        private float Dist(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(
                Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}