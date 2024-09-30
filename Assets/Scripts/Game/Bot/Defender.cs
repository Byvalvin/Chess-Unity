using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Defender: Primarily focuses on protecting the king and avoiding checks, reinforcing a strong defensive strategy.
Focuses on protecting key pieces and minimizing risks
Guardian: Focuses on protecting key pieces, especially the king, by creating a defensive wall.
Wall: Emphasizes solid defenses, making it difficult for opponents to penetrate their setup.
*/
public class DefenderState : BotState
{
    public DefenderState()
    {

    }
    public DefenderState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        return 0; // Placeholder logic; consider capturing pieces, controlling the center, etc.
    }

}
public class Defender : Bot
{
    protected override void Awake()
    {
        state = new DefenderState();
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
