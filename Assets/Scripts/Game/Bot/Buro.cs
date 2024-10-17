using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MCTSNode
{
    private GameState gameState;
    private int playerIndex;
    private int wins = 0;
    private int visits = 0;
    private List<MCTSNode> children = new List<MCTSNode>();
    private Vector2Int[] move; // Changed to hold an array of two Vector2Ints
    private BuroState buroState; // Reference to BuroState
    private MCTSNode parent; // Reference to parent node

    public MCTSNode(GameState gameState, int playerIndex, BuroState buroState, Vector2Int[] move = null)
    {
        this.gameState = gameState;
        this.playerIndex = playerIndex;
        this.buroState = buroState; // Initialize the reference
        this.move = move ?? new Vector2Int[2]; // Initialize move to an empty array if null
    }

    public MCTSNode Select()
    {
        // Selection using UCT
        if (children.Count > 0)
        {
            return children.OrderByDescending(c => c.UCTValue()).First().Select();
        }
        else
        {
            Expand();
            return this;
        }
    }

    private void Expand()
    {
        var possibleMoves = buroState.GenerateAllMoves(gameState, playerIndex); // Use BuroState's method
        foreach (var move in possibleMoves)
        {
            GameState newGameState = gameState.Clone();
            newGameState.MakeBotMove(move[0], move[1]);
            children.Add(new MCTSNode(newGameState, 1 - playerIndex, buroState, move));
        }
    }

    public int Simulate()
    {
        // Simulate a random game from this node's state
        GameState simulatedGame = gameState.Clone();
        while (!IsGameOver(simulatedGame))
        {
            var moves = buroState.GenerateAllMoves(simulatedGame, playerIndex); // Use BuroState's method
            if (moves.Count == 0) break; // No moves available, game over
            var randomMove = moves[Random.Range(0, moves.Count)];
            simulatedGame.MakeBotMove(randomMove[0], randomMove[1]);
        }
        return buroState.EvaluateGameState(simulatedGame); // Use BuroState's method
    }

    public void Backpropagate(int result)
    {
        visits++;
        wins += result;
        // If there's a parent, backpropagate the result
        if (parent != null)
            parent.Backpropagate(result);
    }

    public Vector2Int[] BestMove()
    {
        return children.OrderByDescending(c => c.wins / (float)c.visits).First().move;
    }

    private float UCTValue()
    {
        return (wins / (float)visits) + Mathf.Sqrt(2 * Mathf.Log(parent.visits) / visits);
    }

    private bool IsGameOver(GameState gameState)
    {
        // Implement logic to determine if the game is over
        return gameState.IsGameEnd(); // Placeholder
    }
}

public class BuroState : BotState
{
     private const int KingThreatPenalty = 10;
    private const int Simulations = 1000; // Number of simulations to run for each move

    public BuroState(string playerName, bool colour) : base(playerName, colour) { }
    public BuroState(BuroState original) : base(original) { }
    public override PlayerState Clone() => new BuroState(this);

    public override Vector2Int[] GetMove()
    {
        var rootNode = new MCTSNode(currentGame, TurnIndex, this); // Pass 'this' to MCTSNode
        for (int i = 0; i < Simulations; i++)
        {
            MCTSNode node = rootNode.Select();
            int result = node.Simulate();
            node.Backpropagate(result);
        }
        return rootNode.BestMove();
    }

    public List<Vector2Int[]> GenerateAllMoves(GameState gameState, int playerIndex)
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

    public int EvaluateGameState(GameState gameState)
    {
        int score = 0;
        // Game-ending moves
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

public class Buro : Bot
{
    protected override void Awake()
    {
        // Initialize BuroState if needed
        // state = new BuroState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
