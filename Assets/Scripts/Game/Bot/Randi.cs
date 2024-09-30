using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // for random

/*
Randi: Makes random moves, adding an element of unpredictability.
*/

public class RandiState : BotState
{
    private static readonly System.Random random = new System.Random(); // Singleton instance

    public RandiState(){}
    public RandiState(RandiState original) : base(original){}

    protected override Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        if (moveMap.Count == 0) return null; // No moves available

        // Get a random key from the dictionary
        int randomKeyIndex = random.Next(moveMap.Count);
        Vector2Int randomFrom = default;
        int index = 0;

        foreach (var key in moveMap.Keys)
        {
            if (index == randomKeyIndex)
            {
                randomFrom = key;
                break;
            }
            index++;
        }

        // Get valid moves for the selected key
        var validMoves = moveMap[randomFrom];
        if (validMoves.Count == 0) return null; // No valid moves available for this key

        // Select a random move from the valid moves using an index
        int randomMoveIndex = random.Next(validMoves.Count);
        Vector2Int randomTo = default;
        index = 0;

        foreach (var move in validMoves){
            if (index == randomMoveIndex){
                randomTo = move;
                break;
            }
            index++;
        }

        // Return the random move
        return new Vector2Int[] { randomFrom, randomTo };
    }
}


public class Randi : Bot
{
    protected override void Awake()
    {
        state = new RandiState();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}


