using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Alchemist: Focuses on piece exchanges, valuing trades that lead to a favorable material advantage.
Creates synergies between pieces.  consider capturing pieces, controlling the center, etc.
*/

public class AlchemistState : BotState
{
    public AlchemistState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public AlchemistState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Evaluate potential piece exchange
        score += EvaluatePieceExchange(movingPiece, targetPiece);

        // 2. Central Control
        score += CentralControlBonus(to, clone);

        // 3. Synergies
        score += EvaluatePieceSynergies(from, to);

        // 4. Piece Safety
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 5. Army value impact
        score += ArmyValue(clone, true) - ArmyValue(clone, false);

        // 5. Check if moving piece is defended
        score += PieceDefended(clone, movingPiece, to);

        return score;
    }

    private int EvaluatePieceSynergies(Vector2Int from, Vector2Int to)
    {
        int synergyScore = 0;

        // Example: Reward positioning that allows pieces to protect each other
        // Check for pieces in close proximity that can support each other
        foreach (PieceState piece in currentGame.PlayerStates[TurnIndex].PieceStates)
        {
            if (piece.Position != from)
            {
                // Calculate distance and potential support
                if (Vector2Int.Distance(to, piece.Position) == 1)
                {
                    synergyScore += 2; // Reward close support
                }
            }
        }

        return synergyScore;
    }

    private int EvaluatePieceExchange(PieceState movingPiece, PieceState targetPiece)
    {
        if (targetPiece != null) // Capturing
        {
            return pieceValue[targetPiece.Type] - pieceValue[movingPiece.Type];
        }
        return 0; // No exchange
    }


}
public class Alchemist : Bot
{
    protected override void Awake()
    {
        //state = new AlchemistState();
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
