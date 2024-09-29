using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Focuses on long-term advantages, calculating the best overall moves based on potential outcomes.
Hoarder: Focuses on gathering and controlling resources, ensuring key pieces are well-protected.
*/
public class Strategist : Bot
{
    public Strategist(Strategist original) : base(original) {// Copy constructor
        // Additional initialization for Strategist can be done here if needed
    }
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
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
