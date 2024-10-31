using System;
using System.Collections.Generic;
using UnityEngine;

public class Aggressor : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class AggressorState : BotState
{
    private const int AggressiveBoost = 5,
                    AheadAggressionBonus = 10,
                    BehindCautionPenalty = -10;

    public AggressorState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public AggressorState(AggressorState original) : base(original){
    }
    public override PlayerState Clone() => new AggressorState(this);

    protected override int EvaluateMove(int fromIndex, int toIndex, GameState clone){
        int score = 0;

        // 1. Capture Bonus
        ulong toBitBoard = BitOps.a1 << toIndex;
        if ((toBitBoard & CurrentGame.PlayerStates[1-TurnIndex].OccupancyBoard)!=0)
        {
            PieceBoard targetPiece = CurrentGame.GetPieceBoard(toIndex, CurrentGame.PlayerStates[1-TurnIndex]);
            score += EvaluateCapture(targetPiece, toBitBoard);
        }

        // evaluate clone score
        clone.MakeBotMove(fromIndex, toIndex);

        // piece safety
        PieceBoard movingPiece = CurrentGame.GetPieceBoard(fromIndex, CurrentGame.PlayerStates[TurnIndex]);
        score += EvaluatePieceSafety(movingPiece.Type, clone, BitOps.GetBitBoard(fromIndex), BitOps.GetBitBoard(toIndex));

        score += EvaluateGameState(clone);



        return score;
    }
    protected override int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // Add more evaluation criteria as needed
        // Example: King safety
        score += EvaluateKingSafety(gameState);

        // Example: Control of center squares
        score += EvaluateCenterControl(gameState);

        // mobility
        score += EvaluateMobility(gameState);

        score += 2 * (EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1-TurnIndex));

        // 6. King Attacks
        score += EvaluateKingThreat(gameState);

        score += AdjustAggressiveness(score);

        // Return the final score
        return score;
    }

    private int AdjustAggressiveness(int score)
    {
        int myArmyValue = EvaluateMaterial(CurrentGame, TurnIndex);
        int opponentArmyValue = EvaluateMaterial(CurrentGame, 1 - TurnIndex);

        if (myArmyValue > opponentArmyValue)
            return score + AheadAggressionBonus;
        else if (myArmyValue < opponentArmyValue)
            return score + BehindCautionPenalty;

        return score;
    }


    private int EvaluateCapture(PieceBoard targetPiece, ulong toBitboard)
    {
        int score = pieceValues[targetPiece.Type] + AggressiveBoost;

        int nDefenders = CountDefenders(CurrentGame, toBitboard);
        int nAttackers = CountAttackers(CurrentGame, toBitboard); // Including the moving piece
        score += (AggressiveBoost * (nAttackers - nDefenders));

        return score;
    }
    private int EvaluateKingThreat(GameState clone)
    {
        int checkBonus = clone.PlayerStates[1 - TurnIndex].IsInCheck? 20:0;
        return KingTiles(clone) + checkBonus;
    }

    
    

}
