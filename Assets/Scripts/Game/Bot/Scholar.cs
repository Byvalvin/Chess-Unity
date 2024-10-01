using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Scholar: Analyzes past games to refine strategies, making calculated decisions based on data.
Scribe: Focuses on documenting the game, analyzing moves to adjust strategies based on history.
Finesse: Prefers delicate maneuvers, prioritizing precision and subtlety over brute force.
*/
public class ScholarState : BotState
{
    public ScholarState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public ScholarState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Scholar : Bot
{
    protected override void Awake()
    {
        //state = new ScholarState();
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

