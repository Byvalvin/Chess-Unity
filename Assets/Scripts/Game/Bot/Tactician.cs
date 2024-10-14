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
    public TacticianState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public TacticianState(TacticianState original) : base(original){}
    public override PlayerState Clone() => new TacticianState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Tactician : Bot
{
    protected override void Awake()
    {
        //state = new TacticianState();
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
