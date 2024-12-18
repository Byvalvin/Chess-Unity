using System;
using System.Collections.Generic;
using UnityEngine;


public class Levi : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class LeviState : BotState
{
    private const int MaxDepth = 200,
                      KingThreatPenalty = 10,
                      CaptureScoreMultiplier = 2,
                      PieceProtectionReward = 5;

    public LeviState(string playerName, bool isWhite) : base(playerName, isWhite) { }

    public LeviState(LeviState original) : base(original) { }

    public override PlayerState Clone() => new LeviState(this);

    protected override float EvaluateMove(int fromIndex, int toIndex, GameState clone)
    {
        clone.MakeBotMove(fromIndex, toIndex);
        float moveScore = IterativeDeepeningMinimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite);
        //float movescore = Minimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite);
        // float moveScore = IterativeMinimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite, 1-TurnIndex);
        //Debug.Log("Scoring | " + fromIndex + " to " + toIndex + ": " + moveScore);
        return moveScore;
    }

    private float IterativeDeepeningMinimax(GameState gameState, int maxDepth, float alpha, float beta, bool maximizingPlayer)
    {
        float bestMoveScore = int.MinValue;

        for (int depth = 1; depth <= maxDepth; depth++)
        {
            float score = Minimax(gameState, depth, alpha, beta, maximizingPlayer);
            bestMoveScore = Mathf.Max(bestMoveScore, score);
        }

        return bestMoveScore;
    }


    private float Minimax(GameState gameState, int depth, float alpha, float beta, bool maximizingPlayer){
        string hashKey = gameState.HashD(); // Generate the hash for the current game state
   
        // Check if we have already evaluated this game state
        if (TT.TryGetValue(hashKey, out float cachedValue))
            return cachedValue; // Return the cached evaluation
        
        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        float eval;
        if (maximizingPlayer == IsWhite){
            float maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex)){
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, !IsWhite);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = maxEval;
        }
        else{
            float minEval = int.MaxValue;
            foreach (var move in GenerateAllMoves(gameState, 1 - TurnIndex)){
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, IsWhite);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = minEval;
        }

        // Store the evaluation in the transposition table
        TT[hashKey] = eval;

        return eval;
    }




    // Check if the game is over (end game conditions such as checkmate or stalemate)
    private bool IsGameOver(GameState gameState)
    {
        return gameState.IsGameEnd(); // Placeholder
    }

    // The method to evaluate the state of the game and return a score
    protected override int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // game-ending moves
        score = GameEndingMove(gameState);
        if (score != 0) return score;

        // Evaluate material balance
        score += EvaluateMaterialDiff(gameState);

        // Evaluate piece positioning
        score += EvaluatePositioning(gameState);

        // Evaluate king safety and control
        score += EvaluateKingSafety(gameState);
        score += EvaluateMobilityDiff(gameState);

        return score;
    }

    private int EvaluateMaterialDiff(GameState gameState)
    {
        return 3*(EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex));
    }

    private int EvaluatePositioning(GameState gameState)
    {
        return EvaluateCenterControl(gameState);
    }

    private int EvaluateMobilityDiff(GameState gameState)
    {
        int spaceControl = EvaluateMobility(gameState, TurnIndex),
            enemyControl = EvaluateMobility(gameState, 1 - TurnIndex);

        return 5 * (spaceControl - enemyControl);
    }

    private int EvaluateKingThreat(GameState gameState)
    {
        int safetyScore = 0;
        PlayerState thisPlayer = gameState.PlayerStates[TurnIndex];
        KingBoard thisKing = (thisPlayer.PieceBoards['K'] as KingBoard);
        ulong kingBitBoard = thisKing.Bitboard;
        int kingBitIndex = thisPlayer.GetKingIndex();

        // Check for direct threats to the king
        foreach (PieceBoard pieceBoard in gameState.PlayerStates[1 - TurnIndex].PieceBoards.Values)
        {
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong attackMoves = 0UL;

                // Check if the opponent can directly attack the king
                if (pieceBoard is KingBoard king)
                {
                    attackMoves = king.GetAttackMoves();
                }
                else if (pieceBoard is PawnBoard pawn)
                {
                    attackMoves = pawn.GetAttackMove(kvp.Key);
                }

                if ((kingBitBoard & attackMoves) != 0)
                {
                    safetyScore -= KingThreatPenalty;
                }

                // Check if opponent can attack king's escape moves
                safetyScore -= BitOps.CountSetBits((thisKing.ValidMovesMap[kingBitIndex] & attackMoves)) * KingThreatPenalty;
            }
        }

        return safetyScore;
    }

    // Generate all moves for a given player, ordered by captures first
    private List<Vector2Int> GenerateAllMovesOrdered(GameState gameState, int playerIndex)
    {
        var moves = new List<Vector2Int>();
        var pieceBoards = gameState.PlayerStates[playerIndex].PieceBoards.Values;

        foreach (var pieceBoard in pieceBoards)
        {
            foreach (int pieceIndex in pieceBoard.ValidMovesMap.Keys)
            {
                var validMoves = gameState.GetMovesAllowed(pieceBoard, pieceIndex);
                while (validMoves != 0)
                {
                    ulong bit = validMoves & (~(validMoves - 1)); // Isolate the rightmost set bit
                    int toIndex = BitOps.BitScan(bit); // Get the index of the isolated bit
                    if ((bit & gameState.PlayerStates[1 - TurnIndex].OccupancyBoard) != 0)
                    {
                        // Prioritize capturing moves
                        moves.Insert(0, new Vector2Int(pieceIndex, toIndex));
                    }
                    else
                    {
                        moves.Add(new Vector2Int(pieceIndex, toIndex));
                    }
                    validMoves ^= bit; // Clear the rightmost set bit to continue
                }
            }
        }

        return moves;
    }


}


