using System.Collections.Generic;
using UnityEngine;
using System;

public class Bot : Player
{

    // Add methods for player actions, like making a move, if needed
}


public abstract class BotState : PlayerState
{
    public GameState CurrentGame{get; set;}
    // Transposition table
    protected Dictionary<string, int> TT = new Dictionary<string, int>();

    public BotState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public BotState(BotState original) : base(original){
        this.CurrentGame = original.CurrentGame;
    }
    public abstract override PlayerState Clone();
    

    public virtual Vector2Int GetMove(){
        
        Dictionary<int, ulong> moveMap = new Dictionary<int, ulong>();
        foreach (PieceBoard pieceBoard in PieceBoards.Values){
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong validMoves = CurrentGame.GetMovesAllowed(pieceBoard, kvp.Key);
                if(validMoves!=0) moveMap[kvp.Key] = validMoves;
            }
            
        }

        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        Vector2Int completeMove = Evaluate(moveMap);
    
        return completeMove;
    }
    protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone)=>1;// placeholder assumes all moves are equal but diff bots will have diff scoring
    
    protected virtual Vector2Int Evaluate(Dictionary<int, ulong> moveMap){
        //return new Vector2Int(8,16);

        int bestFromIndex = -1;
        int bestToIndex = -1;
        int bestScore = int.MinValue; // Initialize with the lowest possible score

        // Loop through each piece's valid moves
        foreach (var kvp in moveMap)
        {
            int from = kvp.Key;
            ulong allTo = kvp.Value;

            // Iterate over the bits in the ulong to find all possible destination indices
            while (allTo != 0)
            {
                ulong bit = allTo & (~(allTo - 1)); // Isolate the rightmost set bit
                int toIndex = BitScan(bit); // Get the index of the isolated bit
                
                // Clone the current game state for evaluation
                GameState clonedGame = CurrentGame.Clone();

                // Evaluate the move and get the score
                int score = EvaluateMove(from, toIndex, clonedGame);

                // Update the best score and corresponding indices if this move is better
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFromIndex = from;
                    bestToIndex = toIndex;
                }

                // Clear the rightmost set bit to continue
                allTo ^= bit;
            }
        }
        return new Vector2Int(bestFromIndex, bestToIndex);
    }

    protected int BitScan(ulong bit)
    {
        if (bit == 0)
        {
            throw new ArgumentException("Input must be a non-zero bit.");
        }

        int index = 0;
        while ((bit & 1) == 0)
        {
            bit >>= 1; // Shift right to examine the next bit
            index++;
        }
        return index;
    }


}
