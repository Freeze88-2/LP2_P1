using System.Threading;
using System;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    internal class ZetaAI : AbstractThinker
    {
        private int maxDepth;
        private float pieceValue = 5;

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            maxDepth = 3;
            (FutureMove, float) mov = (NegaMax(board, board.Turn, 0, float.NegativeInfinity, float.PositiveInfinity, ct));
            Console.WriteLine(mov.Item2);
            return mov.Item1;
        }

        private (FutureMove move, float value) NegaMax(Board board, PColor turn, int depth, float alpha, float beta, CancellationToken ct)
        {
            if (board.CheckWinner() != Winner.None)
            {
                if (board.CheckWinner() == turn.ToWinner())
                {
                    return (FutureMove.NoMove, float.PositiveInfinity);
                }
                else if (board.CheckWinner() == turn.Other().ToWinner())
                {
                    return (FutureMove.NoMove, float.NegativeInfinity);
                }
                else
                    return (FutureMove.NoMove, 0);
            }
            else if (depth == maxDepth)
            {
                float v = HeuristicValue(board, turn);
                return (FutureMove.NoMove, v);
            }
            else
            {
                (FutureMove move, float value) bestMove = (FutureMove.NoMove, float.NegativeInfinity);


                for (int i = 0; i < board.cols; i++)
                {
                    for (int b = 0; b < 2; b++)
                    {
                        if (!board.IsColumnFull(i))
                        {
                            FutureMove pos = new FutureMove(i, (PShape)b);

                            if (board.PieceCount(turn, pos.shape) == 0)
                                pos = pos.shape == PShape.Square ? new FutureMove(i, PShape.Round) : new FutureMove(i, PShape.Square);

                            if (board.PieceCount(turn, PShape.Round) == 0 && board.PieceCount(turn, PShape.Square) == 0)
                                return bestMove;

                            board.DoMove(pos.shape, pos.column);

                            float score = -NegaMax(board, turn.Other(), depth + 1, -beta, -alpha, ct).value;

                            board.UndoMove();

                            if (score > bestMove.value)
                            {
                                alpha = score;

                                bestMove = (pos, score);

                                if (alpha >= beta)
                                    return bestMove;
                            }
                        }
                    }
                }
                return bestMove;
            }
        }
        private float HeuristicValue(Board board, PColor turn)
        {
            float boardValue = pieceValue;

            for (int j = 0; j < board.cols; j++)
            {
                for (int f = 0; f < board.rows; f++)
                {
                    if (!board[f, j].HasValue)
                    {
                        if (f - 1 >= 0 &&  j - 1 >= 0 && j + 1 <= board.cols - 1)
                        {
                            if (board[f - 1, j - 1].HasValue && board[f - 1, j + 1].HasValue)
                            {
                                if ((board[f - 1, j - 1].Value.shape == turn.Shape() || board[f - 1, j - 1].Value.color == turn) &&
                                    (board[f - 1, j + 1].Value.shape == turn.Shape() || board[f - 1, j + 1].Value.color == turn))
                                {
                                    boardValue += 10;
                                }
                                if ((board[f - 1, j - 1].Value.shape == turn.Other().Shape() || board[f - 1, j - 1].Value.color == turn.Other()) &&
                                    (board[f - 1, j + 1].Value.shape == turn.Other().Shape() || board[f - 1, j + 1].Value.color == turn.Other()))
                                {
                                    boardValue -= 10;
                                }
                            }
                            else if (board[f - 1, j - 1].HasValue)
                            {
                                if (board[f - 1, j - 1].Value.shape == turn.Shape()
                                    || board[f - 1, j - 1].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 1, j - 1].Value.shape == turn.Other().Shape()
                                    || board[f - 1, j - 1].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                            else if (board[f - 1, j + 1].HasValue)
                            {
                                if (board[f - 1, j + 1].Value.shape == turn.Shape()
                                    || board[f - 1, j + 1].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 1, j + 1].Value.shape == turn.Other().Shape()
                                    || board[f - 1, j + 1].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                        }



                        if (f - 2 >= 0 && j - 1 >= 0 && j + 1 <= board.cols - 1)
                        {
                            if (board[f - 2, j - 1].HasValue && board[f - 2, j + 1].HasValue)
                            {
                                if ((board[f - 2, j + 1].Value.shape == turn.Shape() || board[f - 2, j + 1].Value.color == turn) &&
                                    (board[f - 2, j - 1].Value.shape == turn.Shape() || board[f - 2, j - 1].Value.color == turn))
                                {
                                    boardValue += 10;
                                }
                                if ((board[f - 2, j + 1].Value.shape == turn.Other().Shape() || board[f - 2, j + 1].Value.color == turn.Other()) &&
                                    (board[f - 2, j - 1].Value.shape == turn.Other().Shape() || board[f - 2, j - 1].Value.color == turn.Other()))
                                {
                                    boardValue -= 10;
                                }
                            }
                            else if (board[Math.Max(0, f - 1), Math.Min(board.cols - 1, j + 1)].HasValue)
                            {
                                if (board[f - 1, j + 1].Value.shape == turn.Shape()
                                    || board[f - 1, j + 1].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 1, j + 1].Value.shape == turn.Other().Shape()
                                    || board[f - 1, j + 1].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                            else if (board[Math.Max(0, f - 2), Math.Max(0, j - 1)].HasValue)
                            {
                                if (board[f - 2, j - 1].Value.shape == turn.Shape()
                                    || board[f - 2, j - 1].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 2, j - 1].Value.shape == turn.Other().Shape()
                                    || board[f - 2, j - 1].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                        }



                        if (f - 2 >= 0 && f - 3 >= 0)
                        {
                            if (board[f - 2, j].HasValue && board[f - 3, j].HasValue)
                            {
                                if ((board[f - 2, j].Value.shape == turn.Shape() || board[f - 2, j].Value.color == turn)
                                    && (board[f - 3, j].Value.shape == turn.Shape() || board[f - 3, j].Value.color == turn))
                                {
                                    boardValue += 10;
                                }
                                if ((board[f - 2, j].Value.shape == turn.Other().Shape() || board[f - 2, j].Value.color == turn.Other())
                                    && (board[f - 3, j].Value.shape == turn.Other().Shape() || board[f - 3, j].Value.color == turn.Other()))
                                {
                                    boardValue -= 10;
                                }
                            }
                            else if (board[f - 2, j].HasValue)
                            {
                                if (board[f - 2, j].Value.shape == turn.Shape()
                                    || board[f - 2, j].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 2, j].Value.shape == turn.Other().Shape()
                                    || board[f - 2, j].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                            else if (board[f - 3,j].HasValue)
                            {
                                if (board[f - 3, j].Value.shape == turn.Shape()
                                    || board[f - 3, j].Value.color == turn)
                                {
                                    boardValue += 2;
                                }
                                if (board[f - 3, j].Value.shape == turn.Other().Shape()
                                    || board[f - 3, j].Value.color == turn.Other())
                                {
                                    boardValue -= 2;
                                }
                            }
                        }
                    }
                }
            }
            return (boardValue * 100);
        }
    }
}
