using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Shadow: Specializes in sneaky plays and ambush tactics, making moves that might seem passive but create traps for the opponent.
Uses sneaky tactics and traps.
Prowler: Stealthy and opportunistic, exploiting weak points.
Assassin: Targets weak or exposed pieces, eliminating threats quickly.
*/

public class ShadowState : BotState
{
    public ShadowState(string _playerName, bool _colour) : base(_playerName, _colour){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        // Implement ambush logic
        int score = 0;

        // Check for potential traps (e.g., moving to a position that could fork pieces next turn)
        if (CanCreateAmbush(from, to))
        {
            score -= 10; // Favor ambush moves
        }

        // Consider defensive positions or retreating to bait the opponent
        if (IsDefensiveMove(from, to))
        {
            score += 5; // Penalty for defensive moves, to prioritize aggression
        }

        return score;
    }

    private bool CanCreateAmbush(Vector2Int from, Vector2Int to)
    {
        // Implement logic to check if moving to 'to' creates a potential trap
        // For example, if it opens up a fork on the next move
        return false; // Placeholder
    }

    private bool IsDefensiveMove(Vector2Int from, Vector2Int to)
    {
        // Implement logic to determine if the move is overly defensive
        return false; // Placeholder
    }
}

public class Shadow : Bot
{
    protected override void Awake()
    {
        //state = new ShadowState();
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
