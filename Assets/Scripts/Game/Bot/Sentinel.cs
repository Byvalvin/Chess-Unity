using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Sentinel: Plays defensively but punishes overextensions by the opponent, focusing on counterattacks.
Strong defensive plays, protecting key pieces.
Enforcer: Punishes mistakes heavily, focusing on capitalizing when the opponent makes a blunder.
*/
public class SentinelState : BotState
{
    public SentinelState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public SentinelState(SentinelState original) : base(original){}
    public override PlayerState Clone() => new SentinelState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Sentinel : Bot
{
    protected override void Awake()
    {
        //state = new SentinelState();
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

