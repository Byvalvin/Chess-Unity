using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Tactician: Aims to create tactical combinations, scoring moves that lead to forks or double attacks.
Aims for short-term gains, making calculated plays to capture pieces or position better
like positioning and counters.
*/
public class Tactician : Bot
{
        public Tactician(Tactician original) : base(original) {// Copy constructor
        // Additional initialization for Tactician can be done here if needed
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
