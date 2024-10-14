using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Phalanx: Prioritizes piece formation and support, aiming to keep pieces together for strength and synergy.
Prioritizes defense and protection, focusing on keeping pieces safe and forming strong formations.
Works on solid formations and teamwork
*/
public class PhalanxState : BotState
{
    public PhalanxState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public PhalanxState(PhalanxState original) : base(original){}
    public override PlayerState Clone() => new PhalanxState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Phalanx : Bot
{
    protected override void Awake()
    {
        //state = new PhalanxState();
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

