using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BitOps 
{
    public const ulong a1 = 1UL; // Constant for bit manipulation
    public const int N = Board.N; // Board size

    // Method to calculate the index based on row and column
    public static int GetIndex(int row, int col) => row * N + col; // Adjust for Unity's Y direction
    public static int GetIndex(Vector2Int position) => GetIndex(position.y, position.x);

    // Method to check if an index is within bounds
    public static bool InBounds(int index)=> 0<=index && index<N*N;

    // Move squares forward
    public static int ForwardMove(int index, int steps = 1) => index + (N * steps);

    // Move squares backward
    public static int BackwardMove(int index, int steps = 1) => index - (N * steps);

    // Move squares left
    public static int LeftMove(int index, int steps = 1) => index - steps;

    // Move squares right
    public static int RightMove(int index, int steps = 1) => index + steps;
    
    // Move diagonally up-left
    public static int Diagonal1Move(int index, int steps = 1) => index + (N * steps) - steps; // Up and left
    
    // Move diagonally up-right
    public static int Diagonal2Move(int index, int steps = 1) => index + (N * steps) + steps; // Up and right
    
    // Move diagonally down-left
    public static int Diagonal3Move(int index, int steps = 1) => index - (N * steps) - steps; // Down and left
    
    // Move diagonally down-right
    public static int Diagonal4Move(int index, int steps = 1) => index - (N * steps) + steps; // Down and right
    
}
