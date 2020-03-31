using ColorShapeLinks.Common;
using System;
using System.Collections;
using System.Collections.Generic;

public class ZetaHeuristic
{
    private int inRowFriend;
    private int inRowEnemy;
    private int emptySpcaces;
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

        thisHasPieces = false;
        otherHasPieces = false;

        foreach (IEnumerable rows in positions)
        {
            inRowFriend = 0;
            inRowEnemy = 0;
            emptySpcaces = 0;

            thisHasPieces = false;
            otherHasPieces = false;

            foreach (Pos pos in rows)
            {
                boardValue = EvaluateShapeAndColor(board, pos, turn,
                    boardValue, true);
            }

            inRowFriend = 0;
            inRowEnemy = 0;
            emptySpcaces = 0;

            thisHasPieces = false;
            otherHasPieces = false;

            foreach (Pos pos in rows)
            {
                boardValue = EvaluateShapeAndColor(board, pos, turn,
                    boardValue, false);
            }
        }

        return (boardValue);
    }

    private float EvaluateShapeAndColor(Board board, Pos pos, PColor turn,
        float boardValue, bool color)
    {
        int inSequence = board.piecesInSequence - 1;

        Pos allyPosStart = new Pos();
        Pos allyPosEnd = new Pos();

        Pos otherPosStart = new Pos();
        Pos otherPosEnd = new Pos();

        Pos allyMiddlePiece = new Pos();
        Pos otherMiddlePiece = new Pos();

        if (board[pos.row, pos.col].HasValue)
        {
            if (color ? board[pos.row, pos.col].Value.color == turn :
                board[pos.row, pos.col].Value.shape == turn.Shape())
            {
                inRowFriend++;
                inRowEnemy = 0;
                thisHasPieces = true;
            }
            else
            {
                inRowEnemy++;
                inRowFriend = 0;
                otherHasPieces = true;
            }
        }
        else
        {
            if (!thisHasPieces)
            {
                allyPosStart = pos;
                emptySpcaces -= 1;
            }
            else if (emptySpcaces == 1)
            {
                allyMiddlePiece = pos;
            }
            else if (inRowFriend > 1)
            {
                allyPosEnd = pos;
            }

            if (!otherHasPieces)
            {
                otherPosStart = pos;
                emptySpcaces -= 1;
            }
            else if (emptySpcaces == 1)
            {
                otherMiddlePiece = pos;
            }
            else if (inRowEnemy > 1)
            {
                otherPosEnd = pos;
            }
            emptySpcaces++;
        }

        if (emptySpcaces >= 2)
        {
            if (inRowFriend == inSequence || inRowFriend == inSequence - 1)
            {
                boardValue = CheckPlayersInSequenceAmount(board, boardValue,
                    allyPosStart, allyPosEnd, allyMiddlePiece, inRowFriend);

                thisHasPieces = false;
                inRowFriend = 0;
            }

            if (inRowEnemy == inSequence || inRowEnemy == inSequence - 1)
            {
                boardValue = CheckPlayersInSequenceAmount(board, boardValue,
                    otherPosStart, otherPosEnd, otherMiddlePiece, inRowEnemy);

                otherHasPieces = false;
                inRowEnemy = 0;
            }
        }
        boardValue += inRowFriend * inRowFriend - inRowEnemy * inRowEnemy;
        return boardValue;
    }

    private float CheckPlayersInSequenceAmount(Board board, float boardValue, Pos start, Pos end, Pos mid, int multiplyValue)
    {
        if (start.row - 1 >= 0 && end.row - 1 >= 0)
        {
            if ((!board[start.row, start.col].HasValue && board[start.row - 1, start.col].HasValue)
            && (!board[end.row, end.col].HasValue && board[end.row - 1, end.col].HasValue))
            {
                boardValue += 1000 * multiplyValue;
            }
        }
        else if (start.row - 1 >= 0)
        {
            if (!board[start.row, start.col].HasValue && board[start.row - 1, start.col].HasValue)
            {
                boardValue += 50 * multiplyValue;
            }
        }
        else if (end.row - 1 >= 0)
        {
            if (!board[end.row, end.col].HasValue && board[end.row - 1, end.col].HasValue)
            {
                boardValue += 50 * multiplyValue;
            }
        }
        else if (mid.row - 1 >= 0)
        {
            if (!board[mid.row, mid.col].HasValue && board[mid.row - 1, mid.col].HasValue)
            {
                boardValue += 50 * multiplyValue;
            }
        }
        else if (!board[mid.row, mid.col].HasValue || !board[end.row, end.col].HasValue || !board[start.row, start.col].HasValue)
        {
            boardValue += 10 * multiplyValue;
        }

        return boardValue;
    }

    private float Dist(float x1, float y1, float x2, float y2) =>
        (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
}