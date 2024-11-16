using System;
using System.Collections.Generic;
using UnityEngine;

public class Buro : Bot
{
    // Main bot class, can be extended with player actions or other functionalities.
}

public class MCTSNode
{
    public GameState State { get; private set; }
    public List<MCTSNode> Children { get; private set; }
    public int Visits { get; private set; }
    public float Wins { get; private set; }
    public MCTSNode Parent { get; private set; }

    public MCTSNode(GameState state, MCTSNode parent = null)
    {
        State = state;
        Children = new List<MCTSNode>();
        Visits = 0;
        Wins = 0;
        Parent = parent;
    }

    public void AddChild(MCTSNode child)
    {
        Children.Add(child);
    }

    public void UpdateStats(float result)
    {
        Visits++;
        Wins += result; // Update wins based on the result
    }

    public float WinRate => Visits == 0 ? 0 : Wins / Visits; // Calculate win rate
}

public class BuroState : BotState
{
    private const int KingThreatPenalty = 10;
    private const int PieceProtectionReward = 5;
    private const int SimulationCount = 400; // Number of simulations per move
    
    private int minViability = 0;

    
    private int Phase => 0<=CurrentGame.MoveCount && CurrentGame.MoveCount<=10 ? 0 :
                        11<=CurrentGame.MoveCount && CurrentGame.MoveCount<=30 ? 1 :
                        2;
    private int[] simMaxDepth = {150,200,300};
    public BuroState(string playerName, bool isWhite) : base(playerName, isWhite) { }

    public BuroState(BuroState original) : base(original) { }

    public override PlayerState Clone() => new BuroState(this);

    protected override float EvaluateMove(int fromIndex, int toIndex, GameState clone)
    {
        // Use MCTS to evaluate the move
        clone.MakeBotMove(fromIndex, toIndex);
        float score = RunMCTS(clone);
        //Debug.Log("for "+fromIndex+" "+toIndex+"score: "+score);
        return (score * 100); // Scale score for evaluation
    }

    private float RunMCTS(GameState gameState)
    {
        string key = gameState.HashA();
       if (TT.TryGetValue(key, out var winrate))
       {
           // Use the stored values from the TT
           return winrate;
       }
        MCTSNode root = new MCTSNode(gameState);

        for (int i = 0; i < SimulationCount; i++)
        {
            MCTSNode node = Select(root);
            float result = Simulate(node.State.Clone());
            Backpropagate(node, result);
            //Debug.Log("after sim "+i+"; "+node.Visits+" is root: "+(node==root));
        }

        // store results in TT
        TT[key] = root.Wins/root.Visits;

        // Find the best child based on visit counts
        MCTSNode bestChild = null;
        foreach (var child in root.Children)
        {
            if (bestChild == null || child.Visits > bestChild.Visits)
            {
                bestChild = child;
            }
        }
        return bestChild?.WinRate ?? 0; // Return the win rate of the best move
    }

    private MCTSNode Select(MCTSNode node)
    {
        //Debug.Log("children count"+node.Children.Count+" visit count"+node.Visits);
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
        // Debug.Log($"Generating {moves.Count} moves for state with {node.Visits} visits.");
        
        // Define the percentage of top moves you want to keep
        float topPercentage = 0.4f;  // 30% of the best moves, adjust as needed
        int topMovesCount = Mathf.CeilToInt(moves.Count * topPercentage); // Ensure at least 1 move is kept if percentage is low

        // The exploration factor (probability of adding a move that fails the first criteria)
        float eFactor = 0.1f;  // 10% chance to consider a move that fails the criteria

        // Min-heap or sorted list to store the top moves
        SortedList<int, MCTSNode> topMoves = new SortedList<int, MCTSNode>();

        // Iterate through all possible moves
        foreach (var move in moves)
        {
            GameState clonedState = node.State.Clone();
            clonedState.MakeBotMove(move.x, move.y);
            int childViability = EvaluateGameState(clonedState);

            // Check if the move meets the criteria
            bool meetsCriteria = childViability > minViability;
            
            // If the move meets the viability criterion, or if it's randomly selected for exploration, add it
            if (meetsCriteria || UnityEngine.Random.value < eFactor)
            {
                // Create the child node
                MCTSNode childNode = new MCTSNode(clonedState, node);
                node.AddChild(childNode);
                topMoves[childViability] = childNode;

                // If the list has more than the desired number of top moves, remove the worst
                if (topMoves.Count > topMovesCount)
                {
                    topMoves.Remove(topMoves.Keys[0]); // Remove the least viable (smallest viability)
                }
            }
        }

        // Ensure that at least one move is considered
        if (topMoves.Count > 0)
        {
            // Randomly pick a move from the best moves
            int randomIndex = UnityEngine.Random.Range(0, topMoves.Count);
            MCTSNode selectedNode = topMoves.Values[randomIndex]; // Select a random child from the best moves
            return selectedNode;
        }

        // If no valid moves were found above minViability, return a fallback node (current node)
        return node;
    }

    /*
    Time Complexity: The time complexity remains at 
𝑂
(
𝑛
log
⁡
𝑘
)
O(nlogk), where 
𝑛
n is the number of moves and 
𝑘
k is the number of top moves you're considering (i.e., determined by topPercentage).
Random Check: The random check (UnityEngine.Random.value < eFactor) is efficient and doesn’t significantly add to the complexity, as it’s just a simple comparison.
By introducing the eFactor, you're allowing for a more dynamic exploration without being too narrow-minded, and you're still keeping the overall performance in check. This adjustment gives you a good balance of exploration and exploitation, which is often essential for algorithms like MCTS*/


    private MCTSNode BestChild(MCTSNode node)
    {
        float bestValue = float.NegativeInfinity;
        MCTSNode bestNode = null;

        foreach (var child in node.Children)
        {
            // Ensure we avoid division by zero
            float uctValue = child.WinRate + Mathf.Sqrt(2 * Mathf.Log(node.Visits) / (child.Visits + 1));
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
        int simDepth = 0;

        while (!state.IsGameEnd() && simDepth <= simMaxDepth[Phase])
        {
            var moves = GenerateAllMoves(state, state.currentIndex);
            if (moves.Count == 0) break; // No valid moves
            var randomMove = moves[UnityEngine.Random.Range(0, moves.Count)];
            state.MakeBotMove(randomMove.x, randomMove.y);

            simDepth++;
        }
        return EvaluateGameOutcome(state);
    }

//     private float Simulate(GameState state){
//         int simDepth = 0;
//         while (!state.IsGameEnd() && simDepth <= simMaxDepth[Phase]){  
//             var moves = GenerateAllMoves(state, state.currentIndex);            
//             if (moves.Count == 0) break; 

//             float bestMoveEval = float.NegativeInfinity;
//             Vector2Int bestMove;
//             foreach (var move in moves){
//                 GameState clonedState = state.Clone();  
//                 clonedState.MakeBotMove(move.x, move.y);
//                 int moveEval = EvaluateGameState(clonedState);
//                 if (moveEval > bestMoveEval) { 
//                     bestMoveEval = moveEval;  
//                     bestMove = move;                
//                 }
//                 // Early cutoff based on evaluation (if a decisive position is reached)
//                 // if (Mathf.Abs(bestMoveEval) > 1000) // Arbitrary threshold, adjust as needed
//                 // {
//                 //     return bestMoveEval;
//                 // }
//             
//             }
//             simDepth++;
//         }
//         return EvaluateGameOutcome(state);    
//     }

    private void Backpropagate(MCTSNode node, float result)
    {
        while (node != null)
        {
            node.UpdateStats(result);

            string key = node.State.HashA();// Update TT entry
            TT[key]=result;

            node = node.Parent; // Move to the parent node
        }
    }

    private float EvaluateGameOutcome(GameState gameState)
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
        return (spaceControl - enemyControl);
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

/*
45
11
1:01
1:18
51
1
1:23
2:56
1:05
2:08
2:12
10
9
10
3
*/


/*
1:08
1:08
1:28
13
31
56
22
1:29
8
1:02
1:39
1:25
2
36
37
1:16
1:11 ERROR

*/

/*
32
1:05
23
34
1:11
9
1:32
53
1:25
1:19
12
1:04
1:02
1:07
1:11
1:01
49
4
2
38 MOVES
*/
