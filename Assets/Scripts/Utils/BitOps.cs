using UnityEngine;
using System;

public static class BitOps 
{
    public const ulong a1 = 1UL; // Constant for bit manipulation
    public const int N = Board.N; // Board size

    // Method to calculate the index based on row and column
    public static int GetIndex(int row, int col) => row * N + col; // Adjust for Unity's Y direction
    public static int GetIndex(Vector2Int position) => GetIndex(position.y, position.x);

    // Method to check if an index is within bounds
    public static bool InBounds(int index) => 0 <= index && index < N * N;

    // Move squares forward
    public static int ForwardMove(int index, int steps = 1) => index + (N * steps);

    // Move squares backward
    public static int BackwardMove(int index, int steps = 1) => index - (N * steps);

    // Move squares left
    public static int LeftMove(int index, int steps = 1) => index - steps;

    // Move squares right
    public static int RightMove(int index, int steps = 1) => index + steps;

    // Move diagonally up-left
    public static int Diagonal1Move(int index, int steps = 1) 
    {
        return LeftMove(ForwardMove(index, steps), steps); // Move up and left
    }

    // Move diagonally up-right
    public static int Diagonal2Move(int index, int steps = 1) 
    {
        return RightMove(ForwardMove(index, steps), steps); // Move up and right
    }

    // Move diagonally down-left
    public static int Diagonal3Move(int index, int steps = 1) 
    {
        return LeftMove(BackwardMove(index, steps), steps); // Move down and left
    }

    // Move diagonally down-right
    public static int Diagonal4Move(int index, int steps = 1) 
    {
        return RightMove(BackwardMove(index, steps), steps); // Move down and right
    }

    // Movement validation
    public static bool IsValidMove(int fromIndex, int toIndex)
    {
        // Check if the target index is in bounds
        if (!InBounds(toIndex)) return false;

        // Calculate the row and column of both indices
        int fromRow = fromIndex / N;
        int fromCol = fromIndex % N;
        int toRow = toIndex / N;
        int toCol = toIndex % N;

        // Check horizontal movement
        if (fromRow == toRow)
        {
            // Check that movement is not wrapping around the board
            return Math.Abs(fromCol - toCol) <= (N - 1);
        }

        // Check vertical movement
        if (fromCol == toCol)
        {
            return Math.Abs(fromRow - toRow) <= (N - 1);
        }

        // Diagonal movement (assuming a piece like a bishop or queen)
        return Math.Abs(fromRow - toRow) == Math.Abs(fromCol - toCol);
    }
}
