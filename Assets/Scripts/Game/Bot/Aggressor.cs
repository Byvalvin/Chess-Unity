using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Aggressor: Always looking to capture enemy pieces, favoring aggressive plays.
 Strong offensive moves, aiming for maximum damage.
 Knight: Uses direct and aggressive tactics to pressure opponents, favoring offensive play
*/
public class Aggressor : Bot
{
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; Prioritize capturing pieces or making aggressive moves
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
