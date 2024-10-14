using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Focuses on long-term advantages, calculating the best overall moves based on potential outcomes.
Hoarder: Focuses on gathering and controlling resources, ensuring key pieces are well-protected.
*/
public class StrategistState : BotState
{
    public StrategistState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public StrategistState(StrategistState original) : base(original){}
    public override PlayerState Clone() => new StrategistState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Strategist : Bot
{
    protected override void Awake()
    {
        //state = new StrategistState();
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
