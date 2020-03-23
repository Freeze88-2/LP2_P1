using System;
using System.Threading;

namespace ColorShapeLinks.Common.AI.ZetaAI
{
    internal class ZetaAI : AbstractThinker
    {
        private int maxDepth = 4;
        private float winScore = 100000;

        public override FutureMove Think(Board board, CancellationToken ct) => 
            NegaMax(board.Copy(), board.Turn, 0, float.NegativeInfinity,
                float.PositiveInfinity, ct).move;

        private (FutureMove move, float value) NegaMax(Board board, PColor turn, int depth, float alpha, float beta, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return (FutureMove.NoMove, 0);

            if (board.CheckWinner() != Winner.None)
            {
                if (board.CheckWinner() == turn.ToWinner())
                {
                    return (FutureMove.NoMove, winScore);
                }
                else
                    return (FutureMove.NoMove, -winScore);
            }
            else if (depth == maxDepth)
            {
                return (FutureMove.NoMove, HeuristicValue(board, turn));
            }
            else
            {
                (FutureMove move, float value) bestMove = (FutureMove.NoMove, float.NegativeInfinity);

                for (int i = 0; i < board.cols; i++)
                {
                    if (board.IsColumnFull(i))
                        continue;

                    for (int b = 0; b < 2; b++)
                    {
                        FutureMove pos = new FutureMove(i, (PShape)b);

                        if (board.PieceCount(turn, pos.shape) == 0)
                            continue;
                        //    pos = pos.shape == PShape.Square ? new FutureMove(i, PShape.Round) : new FutureMove(i, PShape.Square);

                        //if (board.PieceCount(turn, PShape.Round) == 0 && board.PieceCount(turn, PShape.Square) == 0)
                        //    return bestMove;

                        board.DoMove(pos.shape, pos.column);

                        float score = -NegaMax(board.Copy(), turn.Other(), depth + 1, -beta, -alpha, ct).value;

                        board.UndoMove();

                        if (score > alpha)
                        {
                            alpha = score;
                            bestMove = (pos, alpha);

                            if (alpha >= beta)
                                return bestMove;
                        }
                    }
                }
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