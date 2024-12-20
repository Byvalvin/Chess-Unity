using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Buro : Bot
{
    // Main bot class, can be extended with player actions or other functionalities.
}

public class MCTSNode
{
    // The game state at this node
    public GameState State;

    // The parent node in the tree
    public MCTSNode Parent;

    // The child nodes representing possible future states
    public List<MCTSNode> Children;

    // The number of times this node has been visited
    public int VisitCount;

    // The number of wins from this node's simulations
    public int WinCount;

    // The UCT value used for selection
    public float UCTValue;

    // The move that led to this state from the parent
    public Vector2Int Move;


    // Constructor for the MCTSNode
    public MCTSNode(GameState state, Vector2Int move = default, MCTSNode parent = null)
    {
        State = state;
        Parent = parent;
        Children = new List<MCTSNode>();
        VisitCount = 0;
        WinCount = 0;
        Move = move; // Store the move that led to this state
    }

    // Compute the UCT value for this node
    public void ComputeUCTValue(float explorationConstant)
    {
        if (VisitCount == 0 || Parent==null)
        {
            UCTValue = float.MaxValue; // Avoid division by zero, this node will be chosen first
        }
        else
        {
            UCTValue = (float)WinCount / VisitCount + explorationConstant * Mathf.Sqrt(Mathf.Log(Parent.VisitCount) / VisitCount);
        }
    }

        // Compute the UCT value for this node
    public float UCT(float explorationConstant)
    {
        // Avoid division by zero (exploration term will be infinite for unvisited nodes)
        if (VisitCount == 0 || Parent == null) return float.MaxValue; // High UCT value to prioritize selection of unvisited nodes

        // Exploitation term: WinRate // Exploration term: sqrt(log(parentVisitCount) / VisitCount)
        float exploitation = (float)WinCount / VisitCount,
            exploration = explorationConstant * Mathf.Sqrt(Mathf.Log(Parent.VisitCount) / VisitCount);

        // Return the sum of the exploitation and exploration terms
        return exploitation + exploration;
    }

    public float WinRate()=> VisitCount==0? 0 : WinCount/VisitCount;
}


public class BuroState : BotState
{
    private const int KingThreatPenalty = 10;
    private const int PieceProtectionReward = 5;
    private const int SimulationCount = 1200; // Number of simulations per move
    
    // private int minViability = 0;

    
    private int Phase => 0<=CurrentGame.MoveCount && CurrentGame.MoveCount<=10 ? 0 :
                        11<=CurrentGame.MoveCount && CurrentGame.MoveCount<=30 ? 1 :
                        2;
    private int[] simMaxDepth = {300,250,200};
    public BuroState(string playerName, bool isWhite) : base(playerName, isWhite) { }

    public BuroState(BuroState original) : base(original) { }

    public override PlayerState Clone() => new BuroState(this);



    protected override Vector2Int Evaluate(Dictionary<int, ulong> moveMap=null){
        MCTSNode rootNode = new MCTSNode(CurrentGame);  // Root node is the current game state
        ExpandNode(rootNode);
        float explorationConstant = 3.0f;
        for (int i = 0; i < SimulationCount; i++)  // Run simulations
        {
            // Debug.Log(i);
            MCTSNode selectedNode = SelectNode(rootNode, explorationConstant);  // Select node based on UCT
            // Debug.Log(selectedNode.Move);
            if (selectedNode.VisitCount > 0 && selectedNode.Children.Count == 0)
            {
                ExpandNode(selectedNode);  // Expand the node (add children for possible moves)
            }
            int result = SimulateRollout(selectedNode.State);  // Simulate a random rollout
            Backpropagate(selectedNode, result);  // Backpropagate the result to the root
        }

        // After all simulations, pick the child with the highest visit count
        MCTSNode bestMoveNode = rootNode.Children.OrderByDescending(n => n.WinRate()+n.UCTValue).First();
        return bestMoveNode.Move;  // Return the move that led to this best node
    }
    private MCTSNode SelectNode(MCTSNode node, float explorationConstant)
{
    MCTSNode selectedNode = node;

    while (selectedNode.Children.Count > 0)  // As long as the node has children (we haven’t reached a leaf)
    {
        // Compute the UCT value for each child
        selectedNode.Children.ForEach(child => child.ComputeUCTValue(explorationConstant));

        selectedNode = selectedNode.Children.OrderByDescending(n => n.UCTValue).First();
    }

    return selectedNode;
}

private void ExpandNode(MCTSNode node)
{
    List<Vector2Int> legalMoves = GenerateAllMoves(node.State, node.State.currentIndex);
    // Debug.Log($"Legal Moves: {legalMoves.Count} for {node.State.currentIndex}");

    foreach (var move in legalMoves)
    {
        GameState newState = node.State.Clone();
        newState.MakeBotMove(move.x, move.y);
        MCTSNode childNode = new MCTSNode(newState, move, node);
        node.Children.Add(childNode);
    }
}


// Simulate a random game from the given state (rollout)
private int SimulateRollout(GameState state)
{
    // Clone the current game state to avoid modifying the original state
    GameState clonedState = state.Clone();

    // Run a random simulation until the game ends
    int simDepth = 0;
    while ( simDepth<=simMaxDepth[Phase] && !clonedState.IsGameEnd())
    {
        // Get the legal moves available in the current state
        List<Vector2Int> legalMoves = GenerateAllMoves(clonedState, clonedState.currentIndex);

        if(legalMoves.Count==0)
            break;
        // Pick a random move from the list of legal moves
        Vector2Int move = RandomMove(legalMoves);
        //Vector2Int move = HeuristicMove(legalMoves, clonedState); //60 to 100 times slower
        
        // Apply the random move to the cloned game state
        clonedState.MakeBotMove(move[0], move[1]);
        // ApplyMove(clonedState, randomMove);

        simDepth++;
    }

    // Return the result of the game (1 for win, -1 for loss, 0 for draw)
    return EvaluateGameOutcome(clonedState);
}

private Vector2Int RandomMove(List<Vector2Int> legalMoves)=>legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
private Vector2Int HeuristicMove(List<Vector2Int> legalMoves, GameState currentGame)
{
    // This will store the move with the highest evaluation score
    Vector2Int bestMove = legalMoves.First();  // Start by assuming the first move is the best
    float bestScore = float.NegativeInfinity;  // Start with the lowest possible score

    // Loop over all legal moves
    foreach (var move in legalMoves)
    {
        // Clone the current game state to simulate the move
        GameState clonedState = currentGame.Clone();

        // Apply the move to the cloned state
        clonedState.MakeBotMove(move.x, move.y);  // or clonedState.ApplyMove(move) depending on your method
        
        // Evaluate the game state after making the move
        // float moveScore = EvaluateGameState(clonedState);  // Assuming EvaluateGameState takes a GameState and returns a score
        // float moveScore = SimpleHeuristic(clonedState);
        float moveScore = IdealHeuristic(clonedState);
        // If the score of this move is better than the current best, update the best move
        if (moveScore > bestScore)
        {
            bestScore = moveScore;
            bestMove = move;
        }
    }

    // Return the best move found
    return bestMove;
}



    // Backpropagate the result of a simulation to update visit counts and win counts
    private void Backpropagate(MCTSNode node, int result)
    {
        MCTSNode currentNode = node;

        while (currentNode != null)  // Move upwards to the root node
        {
            currentNode.VisitCount++;
            currentNode.WinCount += result;  // Update win/loss count

            currentNode = currentNode.Parent;  // Move to the parent node
        }
    }

    private int EvaluateGameOutcome(GameState gameState)
    {
        // Return a value based on the game state (e.g., +1 for win, -1 for loss, 0 for draw)
        if (gameState.IsGameEnd())
        {
            if (gameState.Winner == TurnIndex) return 1; // Win
            if (gameState.Winner == -1) return 0; // Draw
            return -1; // Loss
        }
        return 0; // Not finished
    }

    private int SimpleHeuristic(GameState state)
    {
        int score = GameEndingMove(state);
        if (score != 0) return score;
        // Evaluate material balance
        score += EvaluateMaterialDiff(state);
        score += 2*EvaluatePositioning(state);  // Some basic positional evaluation (control of the center, etc.)
        return score;
    }

    private int IdealHeuristic(GameState state){
        int score = GameEndingMove(state);
        if (score != 0) return score;
        // Evaluate material balance
        score += EvaluateMaterialDiff(state);
        score += 2*EvaluateMobilityDiff(state);  // Just material difference
        score += EvaluatePositioning(state);  // Some basic positional evaluation (control of the center, etc.)
        return score;
    }

    protected override int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        score = GameEndingMove(gameState);
        if (score != 0) return score;

        // Evaluate material balance
        score += EvaluateMaterialDiff(gameState);

        // Evaluate piece positioning
        score += EvaluatePositioning(gameState);

        // Evaluate king safety and control
        score += -1*EvaluateKingSafety(gameState, 1-TurnIndex);
        score += 2*EvaluateMobilityDiff(gameState);

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
        int spaceControl = EvaluateMobility(gameState, TurnIndex);
        int enemyControl = EvaluateMobility(gameState, 1 - TurnIndex);
        return 5*(spaceControl - enemyControl);
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
                ulong attackMoves = pieceBoard is KingBoard king ? king.GetAttackMoves() : (pieceBoard is PawnBoard pawn ? pawn.GetAttackMove(kvp.Key) : 0UL);
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
