using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Oracle: Predicts opponent moves based on previous patterns, aiming to counter effectively.
Scout: Gathers information about the opponent's moves before acting.
Echo: Mimics successful strategies, adapting to the opponent's style.
Gathers intelligence on opponent moves, adapting strategies based on their tactics.
*/
public class OracleState : BotState
{
    public OracleState()
    {

    }
    public OracleState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Oracle : Bot
{
    protected override void Awake()
    {
        state = new OracleState();
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

