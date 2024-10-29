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
    public static int ForwardMove(int index, int steps = 1) => InBounds(index)? index + (N * steps) : -1;

    // Move squares backward
    public static int BackwardMove(int index, int steps = 1) => InBounds(index)? index - (N * steps) : -1;

    // Move squares left
    public static int LeftMove(int index, int steps = 1) => InBounds(index)? index - steps : -1;

    // Move squares right
    public static int RightMove(int index, int steps = 1) => InBounds(index)? index + steps : -1;

    // Move diagonally up-left
    public static int Diagonal1Move(int index, int steps = 1) 
    {
        int fwd = ForwardMove(index, steps),
            lfwd = LeftMove(fwd, steps);
        return InBounds(index) && InBounds(fwd) && isValidDiagonalMove(index, lfwd)? lfwd : -1; // Move up and left
    }

    // Move diagonally up-right
    public static int Diagonal2Move(int index, int steps = 1) 
    {
        int fwd = ForwardMove(index, steps),
            rfwd = RightMove(fwd, steps);
        return InBounds(index) && InBounds(fwd) && isValidDiagonalMove(index, rfwd)? rfwd : -1; // Move up and right
    }

    // Move diagonally down-left
    public static int Diagonal3Move(int index, int steps = 1) 
    {
        int bck = BackwardMove(index, steps),
            lbck = LeftMove(bck, steps);
        return InBounds(index) && InBounds(bck) && isValidDiagonalMove(index, lbck)? lbck : -1; // Move down and left
    }

    // Move diagonally down-right
    public static int Diagonal4Move(int index, int steps = 1) 
    {
        int bck = BackwardMove(index, steps),
            rbck = RightMove(bck, steps);
        return InBounds(index) && InBounds(bck) && isValidDiagonalMove(index, rbck)?  rbck : -1; // Move down and right
    }

    // Method to check if two indices are in a straight line
    public static bool IsSameLine(int fromIndex, int toIndex)
    {
        int fromRow = fromIndex / N;
        int fromCol = fromIndex % N;
        int toRow = toIndex / N;
        int toCol = toIndex % N;

        return isValidHorizontalMove(fromRow, fromCol, toRow, toCol) || 
               isValidVerticalMove(fromRow, fromCol, toRow, toCol) || 
               isValidDiagonalMove(fromRow, fromCol, toRow, toCol);
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
            isValidDiagonalMove(fromRow, fromCol, toRow, toCol) || 
            IsValidKnightMove(fromRow, fromCol, toRow, toCol);
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
    // Diagonal movement 2
    public static bool isValidDiagonalMove(int fromIndex, int toIndex){
        // Calculate the row and column of both indices
        int fromRow = fromIndex / N;
        int fromCol = fromIndex % N;
        int toRow = toIndex / N;
        int toCol = toIndex % N;
        return Math.Abs(fromRow - toRow) == Math.Abs(fromCol - toCol); 
    }

    // Check knight movement
    public static bool IsValidKnightMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(fromRow - toRow);
        int colDiff = Math.Abs(fromCol - toCol);
        return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
    }

    public enum MovementType{
        Diagonal,    
        Horizontal,    
        Vertical,
        
        Bishop,
        Rook,    
        Any
    }



    public static ulong GetPathMask(int checkerIndex, int kingIndex, MovementType movementType){
        ulong pathMask = 0;
        int rowDiff = (kingIndex / 8) - (checkerIndex / 8);
        int colDiff = (kingIndex % 8) - (checkerIndex % 8);
        // Handle specific movement types
        if (movementType == MovementType.Diagonal || movementType == MovementType.Bishop || movementType == MovementType.Any){
            if (Math.Abs(rowDiff) == Math.Abs(colDiff)){
                int stepRow = rowDiff > 0 ? 1 : -1;
                int stepCol = colDiff > 0 ? 1 : -1;
                for (int r = checkerIndex / 8 + stepRow, c = checkerIndex % 8 + stepCol; r != kingIndex / 8 || c != kingIndex % 8;r += stepRow, c += stepCol){
                    int newIndex = r * 8 + c;                
                    pathMask |= (1UL << newIndex);
                }       
            }   
        }
        if (movementType == MovementType.Horizontal || movementType == MovementType.Rook || movementType == MovementType.Any){    
            if (checkerIndex / 8 == kingIndex / 8) // Same row        
            {
                int row = checkerIndex / 8;          
                int step = (kingIndex % 8 > checkerIndex % 8) ? 1 : -1;
                for (int c = (checkerIndex % 8) + step; c != kingIndex % 8; c += step){
                    int newIndex = row * 8 + c;             
                  pathMask |= (1UL << newIndex);       
                }
            }
        }

        if (movementType == MovementType.Vertical || movementType == MovementType.Rook || movementType == MovementType.Any){       
            if (checkerIndex % 8 == kingIndex % 8) // Same column    
            {            
                int col = checkerIndex % 8;         
                int step = (kingIndex / 8 > checkerIndex / 8) ? 1 : -1;
                for (int r = (checkerIndex / 8) + step; r != kingIndex / 8; r += step){  
                    int newIndex = r * 8 + col;            
                    pathMask |= (1UL << newIndex);            
                }
            }
        }


        return pathMask; // Returns 0 if there is no valid path}
    }
}
