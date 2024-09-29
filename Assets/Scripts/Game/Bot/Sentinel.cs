using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Sentinel: Plays defensively but punishes overextensions by the opponent, focusing on counterattacks.
Strong defensive plays, protecting key pieces.
Enforcer: Punishes mistakes heavily, focusing on capitalizing when the opponent makes a blunder.
*/
public class Sentinel : Bot
{
    public Sentinel(Sentinel original) : base(original) {// Copy constructor
        // Additional initialization for Sentinel can be done here if needed
    }
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; Focus on defensive scoring; prefer moves that protect pieces
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
