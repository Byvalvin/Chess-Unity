using System;
using System.Collections.Generic;
using UnityEngine;

public class Randi : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class RandiState : BotState
{
    // Declare the random instance at the class level
    private System.Random random = new System.Random();

    public RandiState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public RandiState(RandiState original) : base(original){
    }
    public override PlayerState Clone() => new RandiState(this);


    //protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone)=>1;// placeholder assumes all moves are equal but diff Randis will have diff scoring
    
    protected override Vector2Int Evaluate(Dictionary<int, ulong> moveMap)
    {
        // Collect all valid moves into a list
        List<Vector2Int> validMoves = new List<Vector2Int>();

        foreach (var kvp in moveMap)
        {
            int from = kvp.Key;
            ulong allTo = kvp.Value;

            // Iterate over the bits in the ulong to find all possible destination indices
            while (allTo != 0)
            {
                ulong bit = allTo & (~(allTo - 1)); // Isolate the rightmost set bit
                int toIndex = BitScan(bit); // Get the index of the isolated bit
                
                // Add the move to the list
                validMoves.Add(new Vector2Int(from, toIndex));

                // Clear the rightmost set bit to continue
                allTo ^= bit;
            }
        }

        // Select a random move if there are valid moves
        if (validMoves.Count > 0)
        {
            int randomIndex = random.Next(validMoves.Count); // Get a random index
            return validMoves[randomIndex]; // Return the random move
        }

        return new Vector2Int(-1, -1); // Return an invalid move if no valid moves exist
    }

}
