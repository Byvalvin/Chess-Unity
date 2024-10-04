using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeviState : BotState
{
    private const int MaxDepth = 5; // You can adjust this for performance vs. accuracy

    public LeviState(string playerName, bool colour) : base(playerName, colour) { }
    public LeviState(BotState original) : base(original) { }

    protected override Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MinValue;

        foreach (var kvp in moveMap)
        {
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value)
            {
                GameState clonedGame = currentGame.Clone(); // Clone the current game state
                clonedGame.MakeBotMove(from, to); // Simulate the move

                // Evaluate using Minimax
                int score = Minimax(clonedGame, MaxDepth, int.MinValue, int.MaxValue, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;
                }
            }
        }

        return new Vector2Int[] { bestFrom, bestTo };
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsGameOver(gameState))
        {
            return EvaluateGameState(gameState);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex))
            {
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move[0], move[1]); // Make the move
                
                int eval = Minimax(clonedGame, depth - 1, alpha, beta, false);
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
                clonedGame.MakeBotMove(move[0], move[1]); // Make the move

                int eval = Minimax(clonedGame, depth - 1, alpha, beta, true);
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
                moves.Add(new Vector2Int[] { piece.Position, to });
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

        // Evaluate king safety
        score += EvaluateKingSafety(gameState);

        // Additional heuristics can be added here
        // e.g., control of center, piece activity, etc.

        return score;
    }

    private int EvaluateMaterial(GameState gameState)
    {
        int materialScore = 0;
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            materialScore += pieceValue[piece.Type]; // Sum values of pieces for the current player
        }

        foreach (PieceState piece in gameState.PlayerStates[1 - TurnIndex].PieceStates)
        {
            materialScore -= pieceValue[piece.Type]; // Subtract values of opponent's pieces
        }

        return materialScore;
    }

    private int EvaluatePositioning(GameState gameState)
    {
        int positionScore = 0;
        // Evaluate how well pieces are positioned
        // e.g., rewarding central control, open files for rooks, etc.
        // Example scoring:
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            positionScore += CentralControlBonus(piece.Position, gameState);
            // Add more evaluation logic based on the piece type and position
        }

        return positionScore;
    }

    private int EvaluateKingSafety(GameState gameState)
    {
        int kingSafetyScore = 0;
        // Assess the safety of the player's king
        // Higher scores for safer positions, lower for exposed positions
        PieceState king = gameState.PlayerStates[TurnIndex].GetKing();
        int defenders = PieceDefended(gameState, king, king.Position);
        kingSafetyScore += defenders * 5; // Example weighting

        return kingSafetyScore;
    }

}


public class Levi : Bot
{
    
    protected override void Awake()
    {
        //state = new AggressorState();
    }
    
    // Start is called before the first frame update
    protected override void Start(){
        
    }

    // Update is called once per frame
    protected override void Update(){
        
    }
}
