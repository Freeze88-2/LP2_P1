using System;
using System.Collections;
using System.Collections.Generic;
using ColorShapeLinks.Common;

public class ZetaHeuristic
{
    private int inRowFriend;
    private int inRowEnemy;
    private int emptySpcaces;
    private bool thisBegin;
    private bool thisEnd;
    private bool otherBegin;
    private bool otherEnd;
    private bool thisHasPieces;
    private bool otherHasPieces;

    public float HeuristicValue(Board board, PColor turn, IEnumerable<Pos>[] positions)
    {
        float boardValue = 0;
        float centerRow = board.rows / 2;
        float centerCol = board.cols / 2;
        float maxPoints = Dist(centerRow, centerCol, 0, 0);

        inRowFriend = 0;
        inRowEnemy = 0;
        emptySpcaces = 0;

        thisBegin = false;
        thisEnd = false;
        otherBegin = false;
        otherEnd = false;
        thisHasPieces = false;
        otherHasPieces = false;

        foreach (IEnumerable rows in positions)
        {
            foreach (Pos pos in rows)
            {
                if (board[pos.row, pos.col].HasValue)
                {
                    if (turn.FriendOf(board[pos.row, pos.col].Value))
                    {
                        boardValue += maxPoints - Dist(centerRow, centerCol,
                            pos.col, pos.row);
                    }
                    else
                    {
                        boardValue -= maxPoints - Dist(centerRow, centerCol,
                            pos.col, pos.row);
                    }
                }
                boardValue = EvaluateShapeAndColor(board, pos, turn,
                    boardValue, true);
                boardValue = EvaluateShapeAndColor(board, pos, turn,
                    boardValue, false);
            }
            boardValue += inRowFriend * inRowFriend - inRowEnemy * inRowEnemy;
        }
        return (boardValue);
    }
    private float EvaluateShapeAndColor(Board board, Pos pos, PColor turn,
        float boardValue, bool color)
    {
        int inSequence = board.piecesInSequence - 1;

        if (board[pos.row, pos.col].HasValue)
        {
            if (color ? board[pos.row, pos.col].Value.color == turn :
                board[pos.row, pos.col].Value.shape == turn.Shape())
            {
                inRowFriend++;
                inRowEnemy = 0;
            }
            else
            {
                inRowEnemy++;
                inRowFriend = 0;
            }
        }
        else
        {
            if (!thisHasPieces)
            {
                thisBegin = true;
            }
            if (inRowFriend == inSequence)
            {
                thisEnd = true;
            }

            if (!otherHasPieces)
            {
                otherBegin = true;
            }

            if (inRowEnemy == inSequence)
            {
                otherEnd = true;
            }
            emptySpcaces++;
        }

        if (emptySpcaces >= 2)
        {
            boardValue += inRowFriend * inRowFriend - inRowEnemy * inRowEnemy;

            if (inRowFriend == inSequence)
            {
                inRowFriend = 0;

                if (thisEnd || thisBegin)
                {
                    boardValue += 50;
                }
                if (thisEnd && thisBegin)
                {
                    boardValue += 1000;
                }
            }
            if (inRowEnemy == inSequence)
            {
                inRowEnemy = 0;

                if (otherEnd || otherBegin)
                {
                    boardValue -= 50;
                }
                if (otherEnd && otherBegin)
                {
                    boardValue -= 1000;
                }
            }
        }
        return boardValue;
    }
    private float Dist(float x1, float y1, float x2, float y2) =>
        (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
}

