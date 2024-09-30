using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Alchemist: Focuses on piece exchanges, valuing trades that lead to a favorable material advantage.
Creates synergies between pieces.
*/

public class AlchemistState : BotState
{
    public AlchemistState()
    {

    }
    public AlchemistState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Alchemist : Bot
{
    protected override void Awake()
    {
        state = new AlchemistState();
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
