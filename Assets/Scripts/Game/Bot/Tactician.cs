using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Tactician: Aims to create tactical combinations, scoring moves that lead to forks or double attacks.
Aims for short-term gains, making calculated plays to capture pieces or position better
like positioning and counters.
*/
public class TacticianState : BotState
{
    public TacticianState()
    {

    }
    public TacticianState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Tactician : Bot
{
    protected override void Awake()
    {
        state = new TacticianState();
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
