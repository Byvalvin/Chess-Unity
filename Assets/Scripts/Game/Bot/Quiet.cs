using System.Collections.Generic;
using UnityEngine;

public class QuietState : BotState
{
    private const int BaseDepth = 3; // Base depth for evaluation
    private const int AdditionalDepth = 2; // Additional depth for capturing moves

    public QuietState(string playerName, bool colour) : base(playerName, colour) { }
    public QuietState(QuietState original) : base(original) { }
    public override PlayerState Clone() => new QuietState(this);

    public override Vector2Int[] GetMove()
    {
        Vector2Int[] bestMove = null;
        int bestScore = int.MinValue;

        // Evaluate all possible moves, focusing on capturing moves first
        foreach (var move in GenerateAllMoves(currentGame, TurnIndex))
        {
            GameState clone = currentGame.Clone();
            clone.MakeBotMove(move[0], move[1]);
            int score = QuiescenceEvaluate(clone, BaseDepth);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int QuiescenceEvaluate(GameState gameState, int depth)
    {
        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        // Evaluate the current game state
        int eval = EvaluateGameState(gameState);
        bool foundCapture = false;

        // Generate capturing moves first
        foreach (var move in GenerateAllCapturingMoves(gameState, TurnIndex))
        {
            foundCapture = true;
            GameState clone = gameState.Clone();
            clone.MakeBotMove(move[0], move[1]);
            eval = Mathf.Max(eval, -QuiescenceEvaluate(clone, depth + AdditionalDepth - 1));
        }

        // If no captures were found, evaluate all other moves
        if (!foundCapture) // No captures found
        {
            foreach (var move in GenerateAllMoves(gameState, TurnIndex))
            {
                GameState clone = gameState.Clone();
                clone.MakeBotMove(move[0], move[1]);
                eval = Mathf.Max(eval, -QuiescenceEvaluate(clone, depth - 1));
            }
        }

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
                moves.Add(new Vector2Int[] { piece.Position, to });
            }
        }

        return moves;
    }

    private List<Vector2Int[]> GenerateAllCapturingMoves(GameState gameState, int playerIndex)
    {
        var capturingMoves = new List<Vector2Int[]>();
        var pieces = gameState.PlayerStates[playerIndex].PieceStates;

        foreach (var piece in pieces)
        {
            var validMoves = gameState.GetMovesAllowed(piece);
            foreach (var to in validMoves)
            {
                PieceState targetPiece = gameState.GetTile(to).pieceState;
                if (targetPiece != null && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour)
                {
                    capturingMoves.Add(new Vector2Int[] { piece.Position, to });
                }
            }
        }

        return capturingMoves;
    }

    private bool IsGameOver(GameState gameState)
    {
        return gameState.IsGameEnd(); // Implement your logic to determine if the game is over
    }

    private int EvaluateGameState(GameState gameState)
    {
        // Implement evaluation logic
        return 0; // Placeholder
    }
}




public class Quiet : Bot
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
