using System.Threading;
using System;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    internal class ZetaAI : AbstractThinker
    {
        private int maxDepth;
        private float pieceValue = 5;
        private float extraPieceValue = 1;
        private PColor player;
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            player = board.Turn;
            maxDepth = 2;
            (FutureMove, float) mov = (NegaMax(board, 0, float.NegativeInfinity, float.PositiveInfinity, ct));
            Console.WriteLine(mov.Item2);
            return mov.Item1;
        }

        private (FutureMove move, float value) NegaMax(Board board, int depth, float alpha, float beta, CancellationToken ct)
        {
           
            if (board.CheckWinner() != Winner.None)
            {
                if (board.CheckWinner() == board.Turn.ToWinner())
                {
                    return (FutureMove.NoMove, float.PositiveInfinity);
                }
                else if (board.CheckWinner() == board.Turn.Other().ToWinner())
                {
                    return (FutureMove.NoMove, float.NegativeInfinity);
                }
                else
                    return (FutureMove.NoMove, 0);
            }
            else if (depth >= maxDepth)
            {
                    float v = heuristicValue(board);
                    //Console.WriteLine("Turn : " + board.Turn.ToString() + "   :   " + (board.Turn == player ? v : -v));
                    return (FutureMove.NoMove, (board.Turn == player ? v : -v));
            }
            else
            {
                (FutureMove move, float value) bestMove = (FutureMove.NoMove, float.NegativeInfinity);

                for (int b = 0; b < 2; b++)
                {
                    for (int i = 0; i < board.cols - 1; i++)
                    {
                        if (!board.IsColumnFull(i))
                        {
                            FutureMove pos;

                            pos = (b == 0 ? new FutureMove(i, PShape.Round) : new FutureMove(i, PShape.Square));

                            //Console.WriteLine(pos.shape);

                            if (board.PieceCount(board.Turn, pos.shape) == 0)
                                pos = pos.shape == PShape.Square ? new FutureMove(i, PShape.Round) : new FutureMove(i, PShape.Square);

                            if (board.PieceCount(board.Turn, PShape.Round) == 0 && board.PieceCount(board.Turn, PShape.Square) == 0)
                                break;

                            float score;

                            board.DoMove(pos.shape, pos.column);

                            score = -NegaMax(board.Copy(), depth +1, -beta, -alpha, ct).value;

                            board.UndoMove();

                            if (score > bestMove.value)
                            {
                                alpha = score;

                                bestMove.value = score;
                                bestMove.move = pos;

                                if (alpha >= beta) return bestMove;
                            }
                        }
                    }
                }
                return bestMove;
            }
        }

        private float heuristicValue(Board board)
        {
            float boardValue = pieceValue;
            for (int j = 0; j < board.cols -1; j++)
            {
                for (int f = 0; f < board.rows; f++)
                {
                    if (!board[f, j].HasValue)
                    {
                        if (board[Math.Max(0, f - 1), j].HasValue)
                        {
                            if (board[Math.Max(0, f - 1), j].Value.shape == board.Turn.Shape()
                                || board[Math.Max(0, f - 1), j].Value.color == board.Turn)
                            {
                                boardValue += 2;
                            }
                            else
                                boardValue -= 1;
                        }
                        if (board[Math.Max(0, f - 1), Math.Max(0, j - 1)].HasValue)
                        {
                            if (board[Math.Max(0, f - 1), Math.Max(0, j - 1)].Value.shape == board.Turn.Shape()
                                || board[Math.Max(0, f - 1), Math.Max(0, j - 1)].Value.color == board.Turn)
                            {
                                boardValue += 1;
                            }
                            else
                                boardValue -= 2;
                        }
                        if (board[Math.Max(0, f - 1), Math.Min(board.cols, j + 1)].HasValue)
                        {
                            if (board[Math.Max(0, f - 1), Math.Min(board.cols, j + 1)].Value.shape == board.Turn.Shape()
                                || board[Math.Max(0, f - 1), Math.Min(board.cols, j + 1)].Value.color == board.Turn)
                            {
                                boardValue += 1;
                            }
                            else
                                boardValue -= 2;
                        }
                        if (board[Math.Max(0, f - 2), Math.Max(0, j - 1)].HasValue)
                        {
                            if (board[Math.Max(0, f - 2), Math.Max(0, j - 1)].Value.shape == board.Turn.Shape()
                                || board[Math.Max(0, f - 2), Math.Max(0, j - 1)].Value.color == board.Turn)
                            {
                                boardValue += 1;
                            }
                            else
                                boardValue -= 2;
                        }
                        if (board[Math.Max(0, f - 2), Math.Max(0, j + 1)].HasValue)
                        {
                            if (board[Math.Max(0, f - 2), Math.Min(board.cols, j + 1)].Value.shape == board.Turn.Shape()
                                || board[Math.Max(0, f - 2), Math.Min(board.cols, j + 1)].Value.color == board.Turn)
                            {
                                boardValue += 1;
                            }
                            else
                                boardValue -= 2;
                        }
                    }
                }
            }
            return (boardValue * 100);
        }
    }
}
