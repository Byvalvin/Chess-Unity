using System.Collections.Generic;
using UnityEngine;

public class QuietState : BotState
{
    private const int BaseDepth = 3; // Base depth for evaluation
    private const int AdditionalDepth = 2; // Additional depth for capturing moves
    private const int KingThreatPenalty = 10;

    public QuietState(string playerName, bool colour) : base(playerName, colour) { }
    public QuietState(QuietState original) : base(original) { }
    public override PlayerState Clone() => new QuietState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        clone.MakeBotMove(from, to);
        return QuiescenceEvaluate(clone, BaseDepth);
    }

    private int QuiescenceEvaluate(GameState gameState, int depth)
    {
        string hashKey = gameState.Hash(); // Generate the hash for the current game state

        // Check if we have already evaluated this game state
        if (TT.TryGetValue(hashKey, out int cachedValue))
            return cachedValue; // Return the cached evaluation

        // Base case checks
        if (depth <= 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        int eval = EvaluateGameState(gameState);
        bool foundCapture = false;

        // Generate capturing moves first
        foreach (var move in GenerateAllMoves(gameState, TurnIndex, true)) // Only critical (capturing) moves
        {
            foundCapture = true;
            GameState clone = gameState.Clone();
            clone.MakeBotMove(move[0], move[1]);
            eval = Mathf.Max(eval, -QuiescenceEvaluate(clone, depth - 1 + AdditionalDepth)); // Ensure depth is not increasing
        }

        // If no captures were found, evaluate all other moves
        if (!foundCapture)
        {
            foreach (var move in GenerateAllMoves(gameState, TurnIndex, false)) // Non-critical moves
            {
                GameState clone = gameState.Clone();
                clone.MakeBotMove(move[0], move[1]);
                eval = Mathf.Max(eval, -QuiescenceEvaluate(clone, depth - 1)); // Ensure depth is not increasing
            }
        }

        // Store the evaluation in the transposition table
        TT[hashKey] = eval;

        return eval;
    }

    private List<Vector2Int[]> GenerateAllMoves(GameState gameState, int playerIndex, bool onlyCritical)
    {
        var moves = new List<Vector2Int[]>();
        var pieces = gameState.PlayerStates[playerIndex].PieceStates;

        foreach (var piece in pieces)
        {
            var validMoves = gameState.GetMovesAllowed(piece);
            foreach (var to in validMoves)
            {
                PieceState targetPiece = gameState.GetTile(to)?.pieceState;

                // Capture move
                if (targetPiece != null && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour)
                {
                    if (onlyCritical)
                    {
                        moves.Add(new Vector2Int[] { piece.Position, to });
                    }
                }
                // Non-capturing move
                else if (!onlyCritical)
                {
                    moves.Add(new Vector2Int[] { piece.Position, to });
                }
            }
        }

        return moves;
    }

    private bool IsGameOver(GameState gameState)
    {
        return gameState.IsGameEnd(); // Implement your logic to determine if the game is over
    }

    private int EvaluateGameState(GameState gameState)
    {
        int score = 0;
        score = GameEndingMove(score, gameState);
        if (score != 0) return score;

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

public class Quiet : Bot
{
    protected override void Awake()
    {
        // Initialize QuietState if needed
        // state = new QuietState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
