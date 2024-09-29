using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Alchemist: Focuses on piece exchanges, valuing trades that lead to a favorable material advantage.
Creates synergies between pieces.
*/
public class Alchemist : Bot
{
    public Alchemist(Alchemist original) : base(original) {// Copy constructor
        // Additional initialization for Alchemist can be done here if needed
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
