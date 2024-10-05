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
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Vengeance: Prioritize captures on opponent's pieces
        score += EvaluateVengeance(movingPiece, targetPiece);

        // 2. Recovery: Assess the potential for comeback strategies
        score += EvaluateRecovery(clone);

        // 3. Sacrifice for advantage
        score += EvaluateSacrifice(from, to, clone);

        // 4. General board control and safety
        score += CentralControlBonus(to, clone);
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);
        score += ArmyValue(clone, TurnIndex) - ArmyValue(clone, 1-TurnIndex);

        return score;
    }

        private int EvaluateVengeance(PieceState movingPiece, PieceState targetPiece)
    {
        if (targetPiece != null)
        {
            // High reward for capturing a piece, especially if it was recently captured
            int revengeValue = pieceValue[targetPiece.Type];
            return revengeValue;
        }
        return 0; // No capture
    }

    private int EvaluateRecovery(GameState clone)
    {
        int recoveryScore = 0;

        // Assess if the bot is down material
        if (ArmyValue(clone, TurnIndex) < ArmyValue(clone, 1-TurnIndex))
        {
            recoveryScore += 5; // Reward for being aware of material disadvantage

            // Encourage aggressive moves to counterattack
            foreach (PieceState piece in clone.PlayerStates[TurnIndex].PieceStates)
            {
                foreach (Vector2Int move in clone.GetMovesAllowed(piece))
                {
                    if (clone.GetTile(move).pieceState != null) // If the move captures a piece
                    {
                        recoveryScore += 2; // Encourage counterattacks
                    }
                }
            }
        }

        return recoveryScore;
    }

    private int EvaluateSacrifice(Vector2Int from, Vector2Int to, GameState clone)
    {
        int sacrificeScore = 0;

        // Check if the move leads to a favorable follow-up
        // For example, if it opens a line for attack or creates a tactical opportunity
        if (IsSacrificeAdvantageous(from, to, clone))
        {
            sacrificeScore += 3; // Arbitrary value for potential follow-up attack
        }

        return sacrificeScore;
    }

    private bool IsSacrificeAdvantageous(Vector2Int from, Vector2Int to, GameState clone)
    {
        // Example condition: If sacrificing leads to an attack on a valuable piece
        PieceState targetPiece = clone.GetTile(to).pieceState;

        if (targetPiece != null)
        {
            // Check if the target piece is of high value and if the sacrifice opens other threats
            if (pieceValue[targetPiece.Type] >= 5) // Arbitrary threshold for valuable pieces
            {
                // Check for potential follow-up threats
                foreach (PieceState piece in clone.PlayerStates[TurnIndex].PieceStates)
                {
                    foreach (Vector2Int move in clone.GetMovesAllowed(piece))
                    {
                        // If the move captures a valuable piece or leads to a check
                        if (clone.GetTile(move).pieceState != null &&
                            pieceValue[clone.GetTile(move).pieceState.Type] >= 5)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
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
