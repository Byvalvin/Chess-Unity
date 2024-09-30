using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Berserker: Takes risks to capture pieces aggressively, even if it means exposing their own. Scores risky captures higher.
Plays recklessly, making bold moves even if they put themselves at risk, emphasizing action over caution.
Makes bold, reckless moves for potential high rewards
Enforcer: Punishes mistakes heavily, focusing on capitalizing when the opponent makes a blunder.
Barrage: Relies on overwhelming force, often sacrificing pieces for a stronger offensive.
Aggressive, prioritizing captures.
*/
public class BerserkerState : BotState
{
    public BerserkerState()
    {
        
    }
    public BerserkerState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Berserker : Bot
{
    protected override void Awake()
    {
        state = new BerserkerState();
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
