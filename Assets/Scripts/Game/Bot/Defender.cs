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
    public DefenderState(string _playerName, bool _colour) : base(_playerName, _colour)
    {

    }
    public DefenderState(BotState botState) : base(botState){}

protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Evaluate King Safety
        score -= KingThreatScore(clone);

        // 2. Piece Protection
        score += EvaluatePieceSafety(from,to, movingPiece.Type, clone);

        // 3. Formation Maintenance
        score += EvaluateFormation(clone);

        // 4. Central Control (optional, can be less of a priority for a Defender)
        score += CentralControlBonus(to, clone);

        // 5. Evaluate potential captures
        if (targetPiece != null)
        {
            score += pieceValue[targetPiece.Type]; // Add score for capturing pieces
            int defended = PieceDefended(currentGame, targetPiece, to);
            if(defended==0){
                score*=2;
            }else{
                score-=20;
            }
        }

        return score;
    }

    private int KingThreatScore(GameState gameState)
    {
        int threatScore = 0;
        Vector2Int kingPosition = GetKing().Position;

        // Assess all possible threats to the king's position
        foreach (var opponentPiece in gameState.PlayerStates[1 - TurnIndex].PieceStates) // Assume opponent is player 1 - TurnIndex
        {
            if (opponentPiece.ValidMoves.Contains(kingPosition))
            {
                threatScore += 10; // High penalty for direct threats
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
            if (pieceInFuture.ValidMoves.Contains( piece.Position) )
            {
                protectionScore += 5; // Reward for protecting a piece
            }
        }

        return protectionScore;
    }

    private int EvaluateFormation(GameState gameState)
    {
        int formationScore = 0;

        // Logic to assess piece formations
        // For example, check if pawns are in a structure, or knights are supporting each other
        // Implement similar checks as seen in the Alchemist bot

        return formationScore;
    }

}
public class Defender : Bot
{
    protected override void Awake()
    {
        //state = new DefenderState();
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
