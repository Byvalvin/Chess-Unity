using System.Collections.Generic;
using UnityEngine;

/*
Avenger: Focuses on vengeance, prioritizing revenge moves against captured pieces.
Phoenix: Emphasizes resilience, with a strategy that allows for comebacks after setbacks.
Can sacrifice pieces for temporary advantages, focusing on regeneration or comeback strategies.
*/

public class AvengerState : BotState
{
    private const int RecoveryAwarenessBonus = 5;
    private const int CounterattackBonus = 2;
    private const int SacrificeBonus = 3;
    private const int ValuablePieceThreshold = 5;

    public AvengerState(string playerName, bool colour) : base(playerName, colour) { }
    public AvengerState(AvengerState original) : base(original) { }
    public override PlayerState Clone() => new AvengerState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        int score = 0;
        PieceState movingPiece = clone.GetTile(from).pieceState;
        PieceState targetPiece = clone.GetTile(to).pieceState;

        string movingPieceType = movingPiece.Type;
        bool movingPieceColour = movingPiece.Colour;

        // Simulate the move
        //GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // game ending moves
        score = GameEndingMove(score, clone);
        if(score!=0) return score;

        // 1. Vengeance: Prioritize captures on opponent's pieces
        score += EvaluateVengeance(targetPiece);

        // 2. Recovery: Assess the potential for comeback strategies
        score += EvaluateRecovery(clone);

        // 3. Sacrifice for advantage
        score += EvaluateSacrifice(from, to, clone);

        // 4. General board control and safety
        score += CentralControlBonus(to, clone);
        score += EvaluatePieceSafety(from, to, movingPieceType, clone);
        score += ArmyValue(clone, TurnIndex) - ArmyValue(clone, 1 - TurnIndex);

        return score;
    }

    private int EvaluateVengeance(PieceState targetPiece)
    {
        if (targetPiece != null)
        {
            // High reward for capturing a piece
            return pieceValue[targetPiece.Type];
        }
        return 0; // No capture
    }

    private int EvaluateRecovery(GameState clone)
    {
        int recoveryScore = 0;

        // Assess if the bot is down material
        if (ArmyValue(clone, TurnIndex) < ArmyValue(clone, 1 - TurnIndex))
        {
            recoveryScore += RecoveryAwarenessBonus; // Reward awareness of material disadvantage

            // Encourage aggressive moves to counterattack
            foreach (PieceState piece in clone.PlayerStates[TurnIndex].PieceStates)
            {
                foreach (Vector2Int move in clone.GetMovesAllowed(piece))
                {
                    if (clone.GetTile(move).pieceState != null) // If the move captures a piece
                    {
                        recoveryScore += CounterattackBonus; // Encourage counterattacks
                    }
                }
            }
        }

        return recoveryScore;
    }

    private int EvaluateSacrifice(Vector2Int from, Vector2Int to, GameState clone)
    {
        if (IsSacrificeAdvantageous(from, to, clone))
        {
            return SacrificeBonus; // Reward potential follow-up attack
        }

        return 0; // No advantage from sacrifice
    }

    private bool IsSacrificeAdvantageous(Vector2Int from, Vector2Int to, GameState clone)
    {
        PieceState targetPiece = clone.GetTile(to).pieceState;

        if (targetPiece != null && pieceValue[targetPiece.Type] >= ValuablePieceThreshold)
        {
            // Check for potential follow-up threats
            foreach (PieceState piece in clone.PlayerStates[TurnIndex].PieceStates)
            {
                foreach (Vector2Int move in clone.GetMovesAllowed(piece))
                {
                    if (clone.GetTile(move).pieceState != null &&
                        pieceValue[clone.GetTile(move).pieceState.Type] >= ValuablePieceThreshold)
                    {
                        return true; // Sacrifice is advantageous
                    }
                }
            }
        }

        return false; // No advantageous sacrifice
    }
}

public class Avenger : Bot
{
    protected override void Awake()
    {
        // Initialize AvengerState if needed
        // state = new AvengerState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
