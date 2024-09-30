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
    public PhalanxState()
    {

    }
    public PhalanxState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Phalanx : Bot
{
    protected override void Awake()
    {
        state = new PhalanxState();
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

