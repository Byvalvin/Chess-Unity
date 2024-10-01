using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Revenant: Focuses on vengeance, prioritizing revenge moves against captured pieces.
Phoenix: Emphasizes resilience, with a strategy that allows for comebacks after setbacks.
Can sacrifice pieces for temporary advantages, with a focus on regeneration or comeback strategies.
*/
public class AvengerState : BotState
{
    public AvengerState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public AvengerState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Avenger : Bot
{
    protected override void Awake()
    {
        //state = new AvengerState();
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
