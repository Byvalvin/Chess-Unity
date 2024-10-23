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

    public static ulong GetBitBoard(int index, ulong bitboard=ulong.MaxValue)=> bitboard & (a1<<index);

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

    // Method to check if two indices are in a straight line
    public static bool IsSameLine(int fromIndex, int toIndex)
    {
        int fromRow = fromIndex / N;
        int fromCol = fromIndex % N;
        int toRow = toIndex / N;
        int toCol = toIndex % N;

        return fromRow == toRow || fromCol == toCol || Math.Abs(fromRow - toRow) == Math.Abs(fromCol - toCol);
    }

    // Get the direction (bitboard) from one index to another
    /*
    Same Row: For a move from index 0 to 7 (first row):
It would set direction to 0xFE (all bits set from 0 to 6).
Same Column: For a move from index 0 to 56 (first column):
It would set direction to 0x0101010101010100 (setting bits for every first column position).
Diagonal: For a move from index 0 to 63 (top-left to bottom-right):
It would set direction to 0x8040201008040200 (all diagonal positions).
    */
    public static ulong GetDirection(int fromIndex, int toIndex)
    {
        ulong direction = 0UL;

        if (fromIndex / N == toIndex / N) // Same row
        {
            int step = fromIndex < toIndex ? 1 : -1;
            for (int i = fromIndex + step; i != toIndex; i += step)
            {
                direction |= a1 << i;
            }
        }
        else if (fromIndex % N == toIndex % N) // Same column
        {
            int step = fromIndex < toIndex ? N : -N;
            for (int i = fromIndex + step; i != toIndex; i += step)
            {
                direction |= a1 << i;
            }
        }
        else if (Math.Abs(fromIndex / N - toIndex / N) == Math.Abs(fromIndex % N - toIndex % N)) // Diagonal
        {
            int rowStep = fromIndex / N < toIndex / N ? N : -N;
            int colStep = fromIndex % N < toIndex % N ? 1 : -1;
            int i = fromIndex + rowStep + colStep;

            while (i != toIndex)
            {
                direction |= a1 << i;
                i += rowStep + colStep;
            }
        }

        return direction; // Return the direction bitboard
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

        return isValidHorizontalMove(fromRow, fromCol, toRow, toCol) || 
               isValidVerticalMove(fromRow, fromCol, toRow, toCol) || 
               isValidDiagonalMove(fromRow, fromCol, toRow, toCol);
    }

    // Check horizontal movement
    public static bool isValidHorizontalMove(int fromRow, int fromCol, int toRow, int toCol) => 
        fromRow == toRow && Math.Abs(fromCol - toCol) <= (N - 1); 

    // Check vertical movement
    public static bool isValidVerticalMove(int fromRow, int fromCol, int toRow, int toCol) => 
        fromCol == toCol && Math.Abs(fromRow - toRow) <= (N - 1); 

    // Diagonal movement
    public static bool isValidDiagonalMove(int fromRow, int fromCol, int toRow, int toCol) => 
        Math.Abs(fromRow - toRow) == Math.Abs(fromCol - toCol); 
}
