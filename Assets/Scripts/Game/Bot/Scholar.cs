using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Scholar: Analyzes past games to refine strategies, making calculated decisions based on data.
Scribe: Focuses on documenting the game, analyzing moves to adjust strategies based on history.
Finesse: Prefers delicate maneuvers, prioritizing precision and subtlety over brute force.
*/
public class Scholar : Bot
{
    public Scholar(Scholar original) : base(original) {// Copy constructor
        // Additional initialization for Scholar can be done here if needed
    }
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; 
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
