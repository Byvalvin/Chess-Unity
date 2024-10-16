using System.Collections.Generic;
using UnityEngine;

/*
Aggressor: Always looking to capture enemy pieces, favoring aggressive plays.
Strong offensive moves, aiming for maximum damage.
*/

public class AggressorState : BotState
{
    private const int AggressiveBoost = 5;
    private const int AheadAggressionBonus = 10;
    private const int BehindCautionPenalty = -10;

    public AggressorState(string playerName, bool colour) : base(playerName, colour) { }
    public AggressorState(AggressorState original) : base(original) { }
    
    public override PlayerState Clone() => new AggressorState(this);

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

        // 1. Capture Bonus
        if (targetPiece != null && targetPiece is not KingState)
        {
            score += EvaluateCapture(targetPiece, to);
        }

        // 2. Central Control
        score += CentralControlBonus(to, clone);

        // 3. Mobility
        score += EvaluateMobility(clone);

        // 4. Piece Safety
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 5. Army Value Comparison
        score += 2 * (ArmyValue(clone, TurnIndex) - ArmyValue(clone, 1 - TurnIndex));

        // 6. King Attacks
        score += EvaluateKingThreat(to, clone);

        // Adjust Aggressiveness
        score = AdjustAggressiveness(score);
        
        return score;
    }

    private int EvaluateCapture(PieceState targetPiece, Vector2Int to)
    {
        int score = pieceValue[targetPiece.Type] + AggressiveBoost;

        int nDefenders = PieceDefended(currentGame, targetPiece, to);
        int nAttackers = 1 + CountAttackers(to); // Including the moving piece
        score += AggressiveBoost * (nAttackers - nDefenders);

        return score;
    }

    private int CountAttackers(Vector2Int targetPosition)
    {
        int attackerCount = 0;
        foreach (PieceState piece in currentGame.PlayerStates[TurnIndex].PieceStates)
        {
            if (piece.ValidMoves.Contains(targetPosition))
                attackerCount++;
        }
        return attackerCount;
    }

    private int EvaluateMobility(GameState clone)
    {
        int mobilityScore = 0;
        foreach (PieceState pieceState in clone.PlayerStates[TurnIndex].PieceStates)
        {
            mobilityScore += pieceState.ValidMoves.Count;
        }
        return mobilityScore;
    }

    private int EvaluateKingThreat(Vector2Int to, GameState clone)
    {
        int checkBonus = currentGame.PlayerStates[1 - TurnIndex].GetKing().Position == to ? 20 : 0;
        return KingTiles(clone) + checkBonus;
    }

    private int AdjustAggressiveness(int score)
    {
        int myArmyValue = ArmyValue(currentGame, TurnIndex);
        int opponentArmyValue = ArmyValue(currentGame, 1 - TurnIndex);

        if (myArmyValue > opponentArmyValue)
            return score + AheadAggressionBonus;
        else if (myArmyValue < opponentArmyValue)
            return score + BehindCautionPenalty;

        return score;
    }
}

public class Aggressor : Bot
{
    protected override void Awake()
    {
        // Initialize AggressorState if needed
        // state = new AggressorState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
