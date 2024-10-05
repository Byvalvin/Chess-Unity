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
    public BerserkerState(string _playerName, bool _colour) : base(_playerName, _colour)
    {
        
    }
    public BerserkerState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Aggressive capture score
        score += EvaluateAggressiveCapture(movingPiece, targetPiece);

        // 2. Risk exposure: Score higher if the move puts the player at risk
        score += EvaluateRisk(from, to, movingPiece, clone);

        // 3. Central Control: Encourage controlling the center
        score += CentralControlBonus(to, clone);

        // 4. Potential for capitalizing on opponent's mistakes
        score += EvaluatePunishment(clone);

        return score;
    }

    private int EvaluateAggressiveCapture(PieceState movingPiece, PieceState targetPiece)
    {
        // Higher score for capturing pieces, regardless of value
        if (targetPiece != null)
        {
            return pieceValue[targetPiece.Type] + 2; // Add a bonus for aggression
        }
        return 0; // No capture
    }

    private int EvaluateRisk(Vector2Int from, Vector2Int to, PieceState movingPiece, GameState clone)
    {
        int riskScore = 0;

        // Check if the move exposes the moving piece to capture
        foreach (PieceState opponentPiece in clone.PlayerStates[1 - TurnIndex].PieceStates)
        {
            if (opponentPiece.ValidMoves.Contains(from))
            {
                riskScore -= 5; // Penalty for moving into danger
            }
        }

        // Encourage bold moves that could lead to high rewards
        if (clone.GetTile(to).pieceState != null) // If capturing a piece
        {
            riskScore += 3; // Reward for taking risks
        }

        return riskScore;
    }

    private int EvaluatePunishment(GameState clone)
    {
        int punishmentScore = 0;

        // Check if the opponent has made a blunder
        foreach (PieceState opponentPiece in clone.PlayerStates[1 - TurnIndex].PieceStates)
        {
            foreach (Vector2Int move in opponentPiece.ValidMoves)
            {
                PieceState targetPiece = clone.GetTile(move).pieceState;
                // If the opponent moves to a position where they can be captured
                if (targetPiece != null && targetPiece.ValidMoves.Count == 1) // Simple blunder condition
                {
                    punishmentScore += pieceValue[targetPiece.Type] + 5; // Reward for capitalizing on mistakes
                }
            }
        }

        return punishmentScore;
    }

}
public class Berserker : Bot
{
    protected override void Awake()
    {
        //state = new BerserkerState();
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
