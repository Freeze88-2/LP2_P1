using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    class ZetaAI : AbstractThinker
    {
        private int maxDepth;

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            maxDepth = 6;
            FutureMove mov = (NegaMax(board, 0, float.NegativeInfinity, float.PositiveInfinity, ct).move);

            return mov;
        }

        private (FutureMove move, float value) NegaMax(Board board, int depth, float alpha, float beta, CancellationToken ct)
        {
            depth++;

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
                else if (board.CheckWinner() == Winner.Draw)
                {
                    return (FutureMove.NoMove, 0);
                }
                else return (FutureMove.NoMove, 0);
            }
            else if(depth > maxDepth)
            {
                return (FutureMove.NoMove, float.NegativeInfinity);
            }
            else
            {
                (FutureMove move, float value) bestMove = (FutureMove.NoMove, float.NegativeInfinity);

                for (int i = 0; i < board.cols; i++)
                {
                    if (!board.IsColumnFull(i))
                    {
                        for (int b = 0; b < 2; b++)
                        {
                            //Console.WriteLine(b);
                            FutureMove pos;
                            pos = b == 0 ? new FutureMove(i, PShape.Round) : new FutureMove(i, PShape.Square);

                            if (pos.shape == PShape.Square)
                                if (board.squarePieces == 0)
                                    pos = new FutureMove(i, PShape.Round);

                            if (pos.shape == PShape.Round)
                                if (board.roundPieces == 0)
                                    pos = new FutureMove(i, PShape.Square);

                            if (board.PieceCount(board.Turn, PShape.Round) == 0 || board.PieceCount(board.Turn, PShape.Square) == 0)
                                return (FutureMove.NoMove, 0);

                            float score;

                            board.DoMove(pos.shape, pos.column);

                            score = -NegaMax(board.Copy(), depth + 1, -alpha, -beta, ct).value;

                            board.UndoMove();

                            if (score >= bestMove.value)
                            {
                                alpha = score;

                                bestMove.value = score;
                                bestMove.move = new FutureMove(i, pos.shape);

                                if (alpha >= beta)
                                {
                                    break;
                                }
                            }
                        }

                    }
                }

                return bestMove;
            }
        }
    }
}

