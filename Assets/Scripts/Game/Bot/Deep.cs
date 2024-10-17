using System.Collections.Generic;
using UnityEngine;

/*
Yes, your current implementation of DeepState effectively incorporates several benefits of iterative deepening. Here's how:

1. Progressive Depth Search
Incremental Depth: Your GetMove() method iterates from depth 1 to MaxDepth, allowing the algorithm to progressively search deeper. This means that even if it doesn't reach the maximum depth within a time constraint, it can return the best move found so far.
2. Move Quality Improvement
Best Move Refinement: As the search progresses to deeper levels, the algorithm updates the bestMove based on scores from deeper evaluations. This allows it to refine its move choice, leading to better decision-making.
3. Transposition Table Utilization
Caching Evaluations: You are using a transposition table to store evaluations of previously encountered game states. This minimizes redundant calculations, enhancing efficiency—especially important in games with high branching factors.
4. Dynamic Adaptation
Immediate Move Return: If there’s a time constraint, you can simply return the best move found at any depth level rather than waiting until the maximum depth is reached, which is beneficial in real-time play.
5. Handling Large Search Spaces
Early Pruning: The use of alpha-beta pruning within your Minimax method allows the algorithm to cut off branches early when they are deemed unpromising. This is particularly effective as the depth increases, leading to a more focused search.
Summary
Your implementation leverages the advantages of iterative deepening through its structure and use of a transposition table. By evaluating moves progressively and caching results, it improves both time efficiency and move quality, aligning well with the principles of iterative deepening in AI search algorithms. To further enhance it, you might consider implementing more specific evaluations for EvaluatePositioning, EvaluateKingSafety, and EvaluateMobility, as these aspects can significantly impact the overall effectiveness of the bot's decision-making.




















The key benefits of using iterative deepening in DeepState compared to the fixed-depth minimax approach in LeviState are:

1. Time Management
Progressive Depth Search: Iterative deepening allows the algorithm to incrementally increase the search depth, which can provide a move quickly even if time is limited. If a time constraint is imposed (e.g., in a real-time game), you can return the best move found so far instead of waiting until the maximum depth is reached.
2. Improved Move Quality
Better Decision-Making: As each depth level is explored, the algorithm refines its move selection based on previously calculated evaluations. This means that if the algorithm runs out of time, it can still return the best move it found at the last completed depth.
3. Efficiency with Transposition Tables
Reuse of Computation: By running the minimax algorithm multiple times with increasing depth, iterative deepening allows for better utilization of the transposition table (TT). Evaluated positions at shallower depths can be reused at deeper levels, which is particularly beneficial in games with high branching factors.
4. Handling Large Search Spaces
Pruning Early: As you discover certain moves to be less promising at lower depths, you can prune those branches sooner. This helps manage large search spaces effectively, as you’re likely to encounter a lot of repetitive states in a typical game.
5. Adaptability
Dynamic Adjustment: If the game environment changes or if a player’s strategy shifts, iterative deepening allows for dynamic re-evaluation. You can adapt to the current game situation by adjusting the depth based on how much time you have left.
Summary
In contrast, a fixed-depth approach like LeviState may miss out on finding the best move early on because it only evaluates until a predetermined depth, which may not provide a satisfactory evaluation in complex scenarios. Iterative deepening addresses this limitation by allowing for a more flexible and robust search strategy that can respond to varying time constraints while improving move quality through repeated evaluations.




*/
public class DeepState : BotState
{
    private const int MaxDepth = 51;
    private const int KingThreatPenalty = 10;
    
    public DeepState(string playerName, bool colour) : base(playerName, colour) { }
    public DeepState(DeepState original) : base(original) { }
    public override PlayerState Clone() => new DeepState(this);

    public override Vector2Int[] GetMove()
    {
        Vector2Int[] bestMove = null;
        int bestScore = int.MinValue;

        for (int depth = 1; depth <= MaxDepth; depth++)
        {
            foreach (var move in GenerateAllMoves(currentGame, TurnIndex))
            {
                GameState clone = currentGame.Clone();
                clone.MakeBotMove(move[0], move[1]);
                int score = Minimax(clone, depth, int.MinValue, int.MaxValue, 1 - TurnIndex);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }

        return bestMove;
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, int maximizingPlayer)
    {
        string hashKey = gameState.Hash(); // Generate hash for the current state

        // Check if we have already evaluated this game state
        if (TT.TryGetValue(hashKey, out int cachedValue))
            return cachedValue; // Return cached evaluation

        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        int eval;
        if (maximizingPlayer == TurnIndex)
        {
            int maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex))
            {
                GameState clone = gameState.Clone();
                clone.MakeBotMove(move[0], move[1]);
                maxEval = Mathf.Max(maxEval, Minimax(clone, depth - 1, alpha, beta, 1 - TurnIndex));
                alpha = Mathf.Max(alpha, maxEval);
                if (beta <= alpha) break; // Beta cut-off
            }
            eval = maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in GenerateAllMoves(gameState, 1 - TurnIndex))
            {
                GameState clone = gameState.Clone();
                clone.MakeBotMove(move[0], move[1]);
                minEval = Mathf.Min(minEval, Minimax(clone, depth - 1, alpha, beta, TurnIndex));
                beta = Mathf.Min(beta, minEval);
                if (beta <= alpha) break; // Alpha cut-off
            }
            eval = minEval;
        }

        // Store the evaluation in the transposition table
        TT[hashKey] = eval;

        return eval;
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
                if (targetPiece != null && targetPiece is not KingState && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour)
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

    private bool IsGameOver(GameState gameState)
    {
        return gameState.IsGameEnd(); // Implement your logic
    }

    private int EvaluateGameState(GameState gameState)
    {
        int score = 0;
        score += EvaluateMaterial(gameState);
        score += EvaluatePositioning(gameState);
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



public class Deep : Bot
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
