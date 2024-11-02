using System;
using System.Collections.Generic;
using UnityEngine;

public class Buro : Bot
{
    // Add methods for player actions, like making a move, if needed
}

public class MCTSNode
{
    public GameState State { get; private set; }
    public List<MCTSNode> Children { get; private set; }
    public int Visits { get; private set; }
    public float Wins { get; private set; }
    public MCTSNode Parent { get; private set; } // Add a reference to the parent node

    public MCTSNode(GameState state, MCTSNode parent = null)
    {
        State = state;
        Children = new List<MCTSNode>();
        Visits = 0;
        Wins = 0;
        Parent = parent; // Set the parent node
    }

    public void AddChild(MCTSNode child)
    {
        Children.Add(child);
    }

    public void UpdateStats(float result)
    {
        Visits++;
        Wins += result;
    }

    public float WinRate => Visits == 0 ? 0 : Wins / Visits;
}

public class BuroState : BotState
{
    private const int KingThreatPenalty = 10,
                PieceProtectionReward = 5;
    private const int SimulationCount = 100; // Number of simulations per move

    public BuroState(string playerName, bool isWhite) : base(playerName, isWhite) { }

    public BuroState(BuroState original) : base(original) { }
    public override PlayerState Clone() => new BuroState(this);

    protected override int EvaluateMove(int fromIndex, int toIndex, GameState clone)
    {
        // Use MCTS to evaluate the move
        clone.MakeBotMove(fromIndex, toIndex);
        float score = RunMCTS(clone);
        return (int)(score * 100); // Scale score for evaluation
    }

    private float RunMCTS(GameState gameState)
    {
        MCTSNode root = new MCTSNode(gameState);

        for (int i = 0; i < SimulationCount; i++)
        {
            MCTSNode node = Select(root);
            float result = Simulate(node.State);
            Backpropagate(node, result);
        }

        // Return the best child based on visit counts
        MCTSNode bestChild = root.Children[0];
        foreach (var child in root.Children)
        {
            if (child.Visits > bestChild.Visits)
            {
                bestChild = child;
            }
        }
        return bestChild.WinRate; // Return the win rate of the best move
    }

    private MCTSNode Select(MCTSNode node)
    {
        while (node.Children.Count > 0)
        {
            node = BestChild(node);
        }
        if (node.Visits == 0)
        {
            return node; // If the node is unvisited, return it for expansion
        }
        return Expand(node);
    }

    private MCTSNode Expand(MCTSNode node)
    {
        var moves = GenerateAllMoves(node.State, node.State.currentIndex);
        Debug.Log(moves.Count + "ggg");
        foreach (var move in moves)
        {
            Debug.Log(move);
            GameState clonedState = node.State.Clone();
            Debug.Log(clonedState.currentIndex);
            clonedState.MakeBotMove(move.x, move.y);
            
            MCTSNode childNode = new MCTSNode(clonedState, node);
            node.AddChild(childNode);
        }
        Debug.Log(node.Children.Count + "childea");
        return node.Children[UnityEngine.Random.Range(0, node.Children.Count)]; // Return a random child
    }

    private MCTSNode BestChild(MCTSNode node)
    {
        float bestValue = float.NegativeInfinity;
        MCTSNode bestNode = null;

        foreach (var child in node.Children)
        {
            float uctValue = child.WinRate + Mathf.Sqrt(2 * Mathf.Log(node.Visits) / child.Visits);
            if (uctValue > bestValue)
            {
                bestValue = uctValue;
                bestNode = child;
            }
        }

        return bestNode;
    }

    private float Simulate(GameState state)
    {
        // Run a random simulation until the game ends
        while (!state.IsGameEnd())
        {
            var moves = GenerateAllMoves(state, state.currentIndex);
            if (moves.Count == 0) break; // No valid moves
            var randomMove = moves[UnityEngine.Random.Range(0, moves.Count)];
            state.MakeBotMove(randomMove.x, randomMove.y);
        }
        return EvaluateGameOutcome(state);
    }

    private void Backpropagate(MCTSNode node, float result)
    {
        while (node != null)
        {
            node.UpdateStats(result);
            node = node.Parent; // Move to the parent node
        }
    }

    private float EvaluateGameOutcome(GameState gameState)
    {
        // Return a value based on the game state (e.g., +1 for win, -1 for loss, 0 for draw)
        if (gameState.IsGameEnd())
        {
            if (gameState.Winner==TurnIndex) return 1; // Win
            if (gameState.Winner == -1) return 0; // Draw
            return -1; // Loss
        }
        return 0; // Not finished
    }

    // Add your existing methods for generating moves and evaluating game states here
    protected override int EvaluateGameState(GameState gameState){
        int score = 0;
        // game ending moves
        //score = GameEndingMove(score, gameState);
        if(score!=0) return score;

        // Evaluate material balance
        score += EvaluateMaterialDiff(gameState);

        // Evaluate piece positioning
        score += EvaluatePositioning(gameState);

        // Evaluate king safety and control
        score += EvaluateKingSafety(gameState);
        score += EvaluateMobilityDiff(gameState);

        return score;
    }

    private int EvaluateMaterialDiff(GameState gameState){
        return EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex);
    }

    private int EvaluatePositioning(GameState gameState){

        return EvaluateCenterControl(gameState);
    }


    private int EvaluateMobilityDiff(GameState gameState){
        int spaceControl = EvaluateMobility(gameState, TurnIndex),
            enemyControl = EvaluateMobility(gameState, 1-TurnIndex);
                
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
        foreach (PieceBoard pieceBoard in gameState.PlayerStates[1 - TurnIndex].PieceBoards.Values){
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong attackMoves = 0UL;
                // Check if the opponent can directly attack the king
                if(pieceBoard is KingBoard king){
                    attackMoves = king.GetAttackMoves();
                }else if (pieceBoard is PawnBoard pawn){
                    attackMoves = pawn.GetAttackMove(kvp.Key);
                }
                if((kingBitBoard & attackMoves)!=0){
                    safetyScore-=KingThreatPenalty;
                }
                
                // Check if opponent can attack king's escape moves
                safetyScore -= BitOps.CountSetBits((thisKing.ValidMovesMap[kingBitIndex] & attackMoves))*KingThreatPenalty;
            }

        }
        
        return safetyScore;
    }
}
