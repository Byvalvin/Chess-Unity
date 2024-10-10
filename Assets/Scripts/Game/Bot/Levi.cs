using System.Collections.Generic;
using UnityEngine;

/*
Strategies for Optimization
- Move Ordering: Prioritize captures and checks to enhance pruning effectiveness.
- Transposition Tables: Store results of previously computed positions.
- Parallel Processing: Utilize if applicable for move evaluations.
- Incremental Search: Build on previous searches for efficiency.
*/

public class LeviState : BotState
{
    private const int MaxDepth = 3;
    private const int KingThreatPenalty = 10;
    private const int CaptureScoreMultiplier = 2;
    private const int PieceProtectionReward = 5;

    public LeviState(string playerName, bool colour) : base(playerName, colour) { }
    public LeviState(BotState original) : base(original) { }

    protected override Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MinValue;

        List<Vector2Int[]> bestMoves = new List<Vector2Int[]>();
        Vector2Int[] best = null;

        foreach (var kvp in moveMap)
        {
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value)
            {
                GameState clonedGame = currentGame.Clone();
                clonedGame.MakeBotMove(from, to);
                int score = Minimax(clonedGame, MaxDepth, int.MinValue, int.MaxValue, Colour);

                Debug.Log($"score eval: {from} -> {to}: {score}");

                if (score > bestScore)
                {
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;
                    bestMoves.Clear();
                    bestMoves.Add(new[] { bestFrom, bestTo });
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(new[] { from, to });
                }
            }
        }

        best = bestMoves.Count > 1 ? bestMoves[Random.Range(0, bestMoves.Count)] : new Vector2Int[] { bestFrom, bestTo };
        PieceState movingPiece = currentGame.GetTile(best[0]).pieceState;
        Debug.Log($"{movingPiece.Type} {movingPiece.Colour} {best[0]} {best[1]} {bestScore}");
        return best;
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsGameOver(gameState))
        {
            return EvaluateGameState(gameState);
        }

        if (maximizingPlayer == Colour)
        {
            int maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex))
            {
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move[0], move[1]);
                int eval = Minimax(clonedGame, depth - 1, alpha, beta, !Colour);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);

                if (beta <= alpha)
                {
                    break; // Alpha-beta pruning
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in GenerateAllMoves(gameState, 1 - TurnIndex))
            {
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move[0], move[1]);
                int eval = Minimax(clonedGame, depth - 1, alpha, beta, Colour);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);

                if (beta <= alpha)
                {
                    break; // Alpha-beta pruning
                }
            }
            return minEval;
        }
    }

    private bool IsGameOver(GameState gameState)
    {
        // Implement logic to determine if the game is over (checkmate, stalemate, etc.)
        return gameState.Checkmate; // Placeholder
    }

    private List<Vector2Int[]> GenerateAllMoves(GameState gameState, int playerIndex)
    {
        var moves = new List<Vector2Int[]>();
        var pieces = gameState.PlayerStates[playerIndex].PieceStates;

        foreach (var piece in pieces)
        {
            var validMoves = gameState.GetMovesAllowed(piece);
            foreach (var to in validMoves)
            {
                PieceState targetPiece = gameState.GetTile(to).pieceState;
                if (targetPiece != null && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour)
                {
                    // Prioritize capturing moves
                    moves.Insert(0, new Vector2Int[] { piece.Position, to });
                }
                else
                {
                    moves.Add(new Vector2Int[] { piece.Position, to });
                }
            }
        }

        return moves;
    }

    private int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // Evaluate material balance
        score += EvaluateMaterial(gameState);

        // Evaluate piece positioning
        score += EvaluatePositioning(gameState);

        // Evaluate king safety and control
        score += EvaluateKingSafety(gameState);
        score += EvaluateMobility(gameState);

        return score;
    }

    private int EvaluateMaterial(GameState gameState)
    {
        return ArmyValue(gameState, TurnIndex) - ArmyValue(gameState, 1 - TurnIndex);
    }

    private int EvaluatePositioning(GameState gameState)
    {
        int positionScore = 0;
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            positionScore += CentralControlBonus(piece.Position, gameState);
            // Additional scoring logic based on piece type and position can be added here
        }

        return positionScore;
    }

    private int EvaluateKingSafety(GameState gameState)
    {
        int safetyScore = 0;
        Vector2Int kingPosition = gameState.PlayerStates[TurnIndex].GetKing().Position;
        HashSet<Vector2Int> kingMoves = gameState.PlayerStates[TurnIndex].GetKing().ValidMoves;

        // Check for direct threats to the king
        foreach (var opponentPiece in gameState.PlayerStates[1 - TurnIndex].PieceStates)
        {
            // Check if the opponent can directly attack the king
            if (opponentPiece.ValidMoves.Contains(kingPosition))
            {
                safetyScore -= KingThreatPenalty * 3; // Higher penalty for being in check
            }

            // Check if opponent can attack king's escape moves
            foreach (var escape in kingMoves)
            {
                if (opponentPiece.ValidMoves.Contains(escape))
                {
                    safetyScore -= KingThreatPenalty; // Penalty for threatening escape routes
                }
            }
        }

        // Additional logic for assessing surrounding piece safety can be added here

        return safetyScore;
    }


    private int EvaluateMobility(GameState gameState)
    {
        int spaceControl = 0, enemyControl = 0;
        foreach (PieceState pieceState in gameState.PlayerStates[TurnIndex].PieceStates)
            spaceControl += pieceState.ValidMoves.Count;
        foreach (PieceState pieceState in gameState.PlayerStates[1 - TurnIndex].PieceStates)
            enemyControl += pieceState.ValidMoves.Count;

        return 5 * (spaceControl - enemyControl);
    }
}

public class Levi : Bot
{
    protected override void Awake()
    {
        // Initialize LeviState if needed
        // state = new LeviState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
