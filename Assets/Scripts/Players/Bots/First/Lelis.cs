using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Lelis : Bot
{
    // Bot class for Lelis.
}

public class LelisState : BotState
{
    private const int MaxDepth = 6;  // Maximum depth to search
    private const int KingThreatPenalty = 10;
    private const int CaptureScoreMultiplier = 2;
    private const int PieceProtectionReward = 5;
    
    private Stopwatch stopwatch = new Stopwatch();  // Stopwatch to track search time
    private TimeSpan maxThinkTime = TimeSpan.FromSeconds(5);  // Max allowed thinking time per move

    public LelisState(string playerName, bool isWhite) : base(playerName, isWhite) {}

    public LelisState(LelisState original) : base(original) {}

    public override PlayerState Clone() => new LelisState(this);
    
    // Override to use Iterative Deepening for move evaluation
    public override Vector2Int GetMove()
    {
        return IterativeDeepeningSearch();
    }

    private Vector2Int IterativeDeepeningSearch()
    {
        Vector2Int bestMove = new Vector2Int();
        float bestScore = float.NegativeInfinity;

        // Start the stopwatch
        stopwatch.Restart();
        
        // Perform iterative deepening: increasing depth limit
        for (int depth = 1; depth <= MaxDepth; depth++)
        {
            // Stop search if we run out of time
            if (stopwatch.Elapsed >= maxThinkTime)
            {
                Console.WriteLine("Time limit reached!");
                break;
            }

            // Perform search at the current depth limit
            float score = IterativeDeepeningSearchAtDepth(depth);

            // If we found a better score, save the best move
            if (score > bestScore)
            {
                bestScore = score;
                //bestMove = bestMove; // Save corresponding move (should capture actual move, not just score)
            }
        }

        // Stop the stopwatch after completing the search
        stopwatch.Stop();

        // Log the time taken for the search
        Console.WriteLine($"Search Time: {stopwatch.ElapsedMilliseconds} ms");

        return bestMove;
    }

    // This method performs Iterative Deepening Search at a specific depth
    private float IterativeDeepeningSearchAtDepth(int depth)
    {
        float bestScore = float.NegativeInfinity;
        List<Vector2Int> possibleMoves = GenerateAllMoves(CurrentGame, TurnIndex);

        // Perform Minimax search at the given depth with alpha-beta pruning
        foreach (var move in possibleMoves)
        {
            GameState clonedGameState = CurrentGame.Clone(); // Clone the CurrentGame before making a move
            clonedGameState.MakeBotMove(move.x, move.y);

            float score = Minimax(clonedGameState, depth, float.NegativeInfinity, float.PositiveInfinity, !IsWhite);
            
            if (score > bestScore)
            {
                bestScore = score;
            }
        }

        return bestScore;
    }

    // Minimax algorithm with Alpha-Beta Pruning, using the Transposition Table for caching
    private float Minimax(GameState gameState, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        string hashKey = gameState.HashD(); // Generate a unique key for the game state

        // Check if the game state has already been evaluated and is in the Transposition Table
        if (TT.TryGetValue(hashKey, out float cachedValue))
        {
            return cachedValue;  // Return cached evaluation from the TT
        }

        if (depth == 0 || gameState.IsGameEnd())
        {
            float eval = EvaluateGameState(gameState);  // Terminal node evaluation
            TT[hashKey] = eval;  // Cache the evaluation in the TT
            return eval;
        }

        float bestValue;
        if (maximizingPlayer)
        {
            bestValue = float.NegativeInfinity;
            List<Vector2Int> moves = GenerateAllMoves(gameState, TurnIndex);

            foreach (var move in moves)
            {
                GameState clonedGameState = gameState.Clone();
                clonedGameState.MakeBotMove(move.x, move.y);

                float score = Minimax(clonedGameState, depth - 1, alpha, beta, false);

                bestValue = Mathf.Max(bestValue, score);
                alpha = Mathf.Max(alpha, bestValue);

                if (beta <= alpha) break;  // Alpha-beta pruning
            }
        }
        else
        {
            bestValue = float.PositiveInfinity;
            List<Vector2Int> moves = GenerateAllMoves(gameState, 1 - TurnIndex);

            foreach (var move in moves)
            {
                GameState clonedGameState = gameState.Clone();
                clonedGameState.MakeBotMove(move.x, move.y);

                float score = Minimax(clonedGameState, depth - 1, alpha, beta, true);

                bestValue = Mathf.Min(bestValue, score);
                beta = Mathf.Min(beta, bestValue);

                if (beta <= alpha) break;  // Alpha-beta pruning
            }
        }

        // Cache the evaluation in the Transposition Table
        TT[hashKey] = bestValue;
        return bestValue;
    }

    // The method to evaluate the state of the game and return a score
    protected override int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // game-ending moves
        // score = GameEndingMove(score, gameState);  // You can add any special evaluation for game-ending moves
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
        return EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex);
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

    // The method to evaluate the king's safety based on potential threats
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
}
