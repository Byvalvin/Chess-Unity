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
    private const int AggressiveCaptureBonus = 2;
    private const int RiskPenalty = -5;
    private const int RiskReward = 3;
    private const int PunishmentReward = 5;

    public BerserkerState(string playerName, bool colour) : base(playerName, colour) { }
    public BerserkerState(BotState botState) : base(botState) { }

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Aggressive capture score
        score += EvaluateAggressiveCapture(targetPiece);

        // 2. Risk exposure
        score += EvaluateRisk(from, to, movingPiece, clone);

        // 3. Central Control
        score += CentralControlBonus(to, clone);

        // 4. Potential for punishing opponent's mistakes
        score += EvaluatePunishment(clone);

        Debug.Log($"{movingPiece.Type} {movingPiece.Colour} {from} {to} {score}");
        
        return score;
    }

    private int EvaluateAggressiveCapture(PieceState targetPiece)
    {
        // Higher score for capturing pieces
        if (targetPiece != null)
        {
            return pieceValue[targetPiece.Type] + AggressiveCaptureBonus;
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
                riskScore += RiskPenalty; // Penalty for moving into danger
            }
        }

        // Reward for taking risks if the move captures a piece
        if (clone.GetTile(to).pieceState != null) // If capturing a piece
        {
            riskScore += RiskReward; // Reward for taking risks
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
                    punishmentScore += pieceValue[targetPiece.Type] + PunishmentReward; // Reward for capitalizing on mistakes
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
        // Initialize BerserkerState if needed
        // state = new BerserkerState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
