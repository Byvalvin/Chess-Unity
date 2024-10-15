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
    private const int KingThreatPenalty = 10;
    private const int CaptureScoreMultiplier = 2;
    private const int PieceProtectionReward = 5;

    public DefenderState(string playerName, bool colour) : base(playerName, colour) { }
    public DefenderState(DefenderState original) : base(original) { }
    public override PlayerState Clone() => new DefenderState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        //GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // game ending moves
        score = GameEndingMove(score, clone);
        if(score!=0) return score;

        // 1. Evaluate King Safety
        score -= KingThreatScore(clone);

        // 2. Piece Protection
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 3. Formation Maintenance
        score += EvaluateFormation(clone);

        // 4. Central Control (optional)
        score += CentralControlBonus(to, clone);

        // 5. Evaluate potential captures
        if (targetPiece != null)
        {
            score += pieceValue[targetPiece.Type]; // Add score for capturing pieces
            int defended = PieceDefended(currentGame, targetPiece, to);
            if (defended == 0)
            {
                score *= CaptureScoreMultiplier; // High value if undefended
            }
            else
            {
                score -= 20; // Penalty for capturing defended pieces
            }
        }
        // 6. Attack King
        score += AttackedKingTiles(clone);
        return score;
    }

    private int KingThreatScore(GameState gameState)
    {
        int threatScore = 0;
        Vector2Int kingPosition = GetKing().Position;

        // Assess threats to the king's position
        foreach (var opponentPiece in gameState.PlayerStates[1 - TurnIndex].PieceStates)
        {
            if (opponentPiece.ValidMoves.Contains(kingPosition))
            {
                threatScore += KingThreatPenalty; // High penalty for direct threats
            }
        }

        return threatScore;
    }

    private int EvaluatePieceProtection(GameState gameState, Vector2Int to)
    {
        int protectionScore = 0;
        PieceState pieceInFuture = gameState.GetTile(to).pieceState;

        // Check if moving to a position that protects key pieces
        foreach (var piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            if (pieceInFuture.ValidMoves.Contains(piece.Position))
            {
                protectionScore += PieceProtectionReward; // Reward for protecting a piece
            }
        }

        return protectionScore;
    }

    private int EvaluateFormation(GameState gameState)
    {
        int formationScore = 0;

        // Logic to assess piece formations (expand this as needed)
        // Example: Check if pawns are in a solid structure
        // Additional checks for knight or bishop support can be added here

        return formationScore;
    }
}

public class Defender : Bot
{
    protected override void Awake()
    {
        // Initialize DefenderState if needed
        // state = new DefenderState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
