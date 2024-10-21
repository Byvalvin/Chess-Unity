using UnityEngine;
using System;

public static class BitOps 
{
    public const ulong a1 = 1UL; // Constant for bit manipulation
    public const int N = Board.N; // Board size

    // Method to calculate the index based on row and column
    public static int GetIndex(int row, int col) => row * N + col; // Adjust for Unity's Y direction
    public static int GetIndex(Vector2Int position) => GetIndex(position.y, position.x);

    // Method to get a Vector2Int from an index
    public static Vector2Int GetPosition(int index)
    {
        int row = index / N;
        int col = index % N;
        return new Vector2Int(col, row); // Adjust for Unity's Y direction
    }



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

    // Check horizontal movement
    public static bool isValidHorizontalMove(int fromRow, int fromCol, int toRow, int toCol) => fromRow==toRow && Math.Abs(fromCol - toCol) <= (N - 1); // Check that movement is not wrapping around the board
    // Check vertical movement
    public static bool isValidVerticalMove(int fromRow, int fromCol, int toRow, int toCol) => fromCol==toCol && Math.Abs(fromRow - toRow) <= (N - 1); // Check that movement is not wrapping around the board
    // Diagonal movement (assuming a piece like a bishop or queen)
    public static bool isValidDiagonalMove(int fromRow, int fromCol, int toRow, int toCol) => Math.Abs(fromRow - toRow) == Math.Abs(fromCol - toCol); // Check that movement is not wrapping around the board
    public static bool IsValidMove(int fromIndex, int toIndex)
    {
        // Check if the target index is in bounds
        if (!InBounds(toIndex)) return false;

        // Calculate the row and column of both indices
        int fromRow = fromIndex / N;
        int fromCol = fromIndex % N;
        int toRow = toIndex / N;
        int toCol = toIndex % N;

        return isValidHorizontalMove(fromRow, fromCol, toRow, toCol) || isValidVerticalMove(fromRow, fromCol, toRow, toCol) || isValidDiagonalMove(fromRow, fromCol, toRow, toCol);


    }
}
