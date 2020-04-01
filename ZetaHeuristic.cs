﻿using ColorShapeLinks.Common;
using System;
using System.Collections;
using System.Collections.Generic;

public class ZetaHeuristic
{
    // Number of pieces in row of this turn
    private int inRowFriend;
    // Number of pieces in row of the other turn
    private int inRowEnemy;
    // Number of empty spaces 
    private int emptySpcaces;

    /// <summary>
    /// Gets the heuristic value of the board received
    /// </summary>
    /// <param name="board"> The current board </param>
    /// <param name="turn"> Which turn it is </param>
    /// <param name="positions"> An array on winCorridors </param>
    /// <returns></returns>
    public float HeuristicValue(Board board, PColor turn,
        IEnumerable<Pos>[] positions)
    {
        // Stores the value of the board
        float boardValue = 0;

        // A loop for every array in positions
        foreach (IEnumerable rows in positions)
        {
            // Sets the number of pieces of this turn to 0
            inRowFriend = 0;
            // Sets the number of pieces of the other turn to 0
            inRowEnemy = 0;
            // Set's the number of empty spaces to 0
            emptySpcaces = 0;

            // A loop for every Pos in the array of positions
            foreach (Pos pos in rows)
            {
                // Sets the boardValue to what's returned by the function
                boardValue = CheckInRowAmount(board, pos, turn,
                    boardValue, true);
            }

            // Resets the value of in row pieces of this turn
            inRowFriend = 0;
            // Resets the value of in row pieces of the other turn
            inRowEnemy = 0;
            // Resets the number of empty spaces
            emptySpcaces = 0;

            // A loop for every Pos in the array of positions
            foreach (Pos pos in rows)
            {
                // Sets the boardValue to what's returned by the function
                boardValue = CheckInRowAmount(board, pos, turn,
                    boardValue, false);
            }
        }
        // Returns the AI the heuristic value of the board
        return boardValue;
    }

    /// <summary>
    /// Checks how many pieces are in a row of the same color or shape
    /// </summary>
    /// <param name="board"> The current board </param>
    /// <param name="pos"> The position being evaluated </param>
    /// <param name="turn"> Which turn it is </param>
    /// <param name="boardValue"> The current value of the board </param>
    /// <param name="color"> If it should check color or shape </param>
    /// <returns> The value of the board </returns>
    private float CheckInRowAmount(Board board, Pos pos, PColor turn,
        float boardValue, bool color)
    {
        // Variable for storing the number of pieces in sequence -1
        int inSequence = board.piecesInSequence - 1;

        // Stores the empty space at the start of the row of this turn
        Pos allyPosStart = new Pos();
        // Stores the empty space at the end of the row of this turn
        Pos allyPosEnd = new Pos();

        // Stores the empty space at the start of the row of the other turn
        Pos otherPosStart = new Pos();
        // Stores the empty space at the end of the row of the other turn
        Pos otherPosEnd = new Pos();

        // Stores the empty space in the middle of a row of this turn
        Pos allyMiddlePiece = new Pos();
        // Stores the empty space in the middle of a row of the other turn
        Pos otherMiddlePiece = new Pos();

        // Checks if the board has value at the give position
        if (board[pos.row, pos.col].HasValue)
        {
            // Checks if it should check the color or the shape and if that
            // color or shape is the same as this turn
            if (color ? board[pos.row, pos.col].Value.color == turn :
                board[pos.row, pos.col].Value.shape == turn.Shape())
            {
                // If it is increments in row of this turn by one
                inRowFriend++;
                // Resets the other turn in row to 0
                inRowEnemy = 0;
            }
            else
            {
                // If it isn't increments in row of the other turn
                inRowEnemy++;
                // Resets this turn in row to 0
                inRowFriend = 0;
            }
        }
        // If it doesn«t have value
        else
        {
            // Checks if this turn already has pieces in row
            if (inRowFriend == 0)
            {
                // Sets the postion of the starting empty space to the pos
                allyPosStart = pos;
                // Subtracts one to the number of empty spaces
                emptySpcaces -= 1;
            }
            // If there's only one empty space sets the middle space to pos
            else if (emptySpcaces == 1) allyMiddlePiece = pos;
            // If there's already a piece in row sets the end pos to the pos
            else if (inRowFriend > 1) allyPosEnd = pos;

            // Checks if the other turn already has pieces in row
            if (inRowEnemy == 0)
            {
                // Sets the postion of the starting empty space to the pos
                otherPosStart = pos;
                // Subtracts one to the number of empty spaces
                emptySpcaces -= 1;
            }
            // If there's only one empty space sets the middle space to pos
            else if (emptySpcaces == 1) otherMiddlePiece = pos;
            // If there's already a piece in row sets the end pos to the pos
            else if (inRowEnemy > 1) otherPosEnd = pos;

            // Increments the number of empty spaces by one
            emptySpcaces++;
        }

        // Checks if there's more than 1 empty spaces
        if (emptySpcaces >= 2)
        {
            // Cheks if the number in row of this turn is the same of in
            // Sequence or insequence minus one
            if (inRowFriend == inSequence || inRowFriend == inSequence - 1)
            {
                // Checks how good the sequence is and assigns it to board
                boardValue = CheckInRowViability(board, boardValue,
                    allyPosStart, allyPosEnd, allyMiddlePiece, inRowFriend);

                // Rests the inrow to zero
                inRowFriend = 0;
            }
            // Cheks if the number in row of the other turn is the same of in
            // Sequence or insequence minus one
            if (inRowEnemy == inSequence || inRowEnemy == inSequence - 1)
            {
                boardValue = CheckInRowViability(board, boardValue,
                    otherPosStart, otherPosEnd, otherMiddlePiece, inRowEnemy);

                // Rests the inrow to zero
                inRowEnemy = 0;
            }
        }
        // In case the rows don't have viability assigns the in row of this
        // turn minus the in row of the other turn
        boardValue += inRowFriend * inRowFriend - inRowEnemy * inRowEnemy;
        // Returns the value of the board
        return boardValue;
    }

    /// <summary>
    /// Checks how good the row is
    /// </summary>
    /// <param name="board"> The current board </param>
    /// <param name="boardValue"> The current value of the board </param>
    /// <param name="start"> The empty space at the start of the row </param>
    /// <param name="end"> The empty space at the end of the row </param>
    /// <param name="mid"> The empty space in the middle of the row </param>
    /// <param name="multiplyValue"> The value multiplier </param>
    /// <returns> The value of the board </returns>
    private float CheckInRowViability(Board board, float boardValue,
        Pos start, Pos end, Pos mid, int multiplyValue)
    {
        // Checks if the start and end with one subtracted is 0 or bigger
        if (start.row - 1 >= 0 && end.row - 1 >= 0)
        {
            // Checks if the start and end is empty and below that is a piece
            if ((!board[start.row, start.col].HasValue && board[start.row - 1,
                start.col].HasValue) && (!board[end.row, end.col].HasValue &&
                board[end.row - 1, end.col].HasValue))
            {
                // Adds 1000 multiplied my the pieces in row to the boardValue
                boardValue += 1000 * multiplyValue;
            }
        }
        // Checks if the start with one subtracted is 0 or bigger
        else if (start.row - 1 >= 0)
        {
            // Checks if start is empty and the below that is a piece
            if (!board[start.row, start.col].HasValue && board[start.row - 1,
                start.col].HasValue)
            {
                // Adds 50 multiplied my the pieces in row to the boardValue
                boardValue += 50 * multiplyValue;
            }
        }
        // Checks if the end with one subtracted is 0 or bigger
        else if (end.row - 1 >= 0)
        {
            // Checks if end is empty and the below that is a piece
            if (!board[end.row, end.col].HasValue && board[end.row - 1,
                end.col].HasValue)
            {
                // Adds 50 multiplied my the pieces in row to the boardValue
                boardValue += 50 * multiplyValue;
            }
        }
        // Checks if the middle with one subtracted is 0 or bigger
        else if (mid.row - 1 >= 0)
        {
            // Checks if middle is empty and the below that is a piece
            if (!board[mid.row, mid.col].HasValue && board[mid.row - 1,
                mid.col].HasValue)
            {
                // Adds 50 multiplied my the pieces in row to the boardValue
                boardValue += 50 * multiplyValue;
            }
        }
        // checks if any of the positions are empty spaces
        else if (!board[mid.row, mid.col].HasValue 
            || !board[end.row, end.col].HasValue 
            || !board[start.row, start.col].HasValue)
        {
            // Adds 10 multiplied my the pieces in row to the boardValue
            boardValue += 10 * multiplyValue;
        }

        // Resturns the value of the board
        return boardValue;
    }
}