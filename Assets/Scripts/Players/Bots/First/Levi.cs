using System;
using System.Collections.Generic;
using UnityEngine;


public class Levi : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class LeviState : BotState
{
    private const int MaxDepth = 4,
                      KingThreatPenalty = 10,
                      CaptureScoreMultiplier = 2,
                      PieceProtectionReward = 5;

    public LeviState(string playerName, bool isWhite) : base(playerName, isWhite) { }

    public LeviState(LeviState original) : base(original) { }

    public override PlayerState Clone() => new LeviState(this);

    // The new iterative Minimax algorithm with TT caching
    protected override float EvaluateMove(int fromIndex, int toIndex, GameState clone)
    {
        clone.MakeBotMove(fromIndex, toIndex);
        float movescore = Minimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite);
        //float moveScore = IterativeMinimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite, 1-TurnIndex);
        //Debug.Log("Scoring | " + fromIndex + " to " + toIndex + ": " + moveScore);
        return movescore;
    }

    private float Minimax(GameState gameState, int depth, float alpha, float beta, bool maximizingPlayer){
        string hashKey = gameState.HashA(); // Generate the hash for the current game state
        //ulong hashKey = gameState.HashB(); // Generate the hash for the current game state
        /*
        Debug.Log(maximizingPlayer+" "+depth + " "+ alpha + " " + beta);
        Debug.Log(TT + "for tt" + hashKey);
        */
        /*
        foreach (var item in TT){
            Debug.Log("Hasing: "+ item.Key + " " + item.Value);
        }
        */
        
        // Check if we have already evaluated this game state
        if (TT.TryGetValue(hashKey, out float cachedValue))
            return cachedValue; // Return the cached evaluation
        
        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        float eval;
        if (maximizingPlayer == IsWhite){
            float maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex)){
                //Debug.Log(move);
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, !IsWhite);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = maxEval;
        }
        else{
            float minEval = int.MaxValue;
            foreach (var move in GenerateAllMoves(gameState, 1 - TurnIndex)){
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, IsWhite);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = minEval;
        }

        // Store the evaluation in the transposition table
        TT[hashKey] = eval;

        return eval;
    }




    // Check if the game is over (end game conditions such as checkmate or stalemate)
    private bool IsGameOver(GameState gameState)
    {
        return gameState.IsGameEnd(); // Placeholder
    }

    // The method to evaluate the state of the game and return a score
    protected override int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // game-ending moves
        score = GameEndingMove(gameState);
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
        return 3*(EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex));
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

    // Generate all moves for a given player, ordered by captures first
    private List<Vector2Int> GenerateAllMovesOrdered(GameState gameState, int playerIndex)
    {
        var moves = new List<Vector2Int>();
        var pieceBoards = gameState.PlayerStates[playerIndex].PieceBoards.Values;

        foreach (var pieceBoard in pieceBoards)
        {
            foreach (int pieceIndex in pieceBoard.ValidMovesMap.Keys)
            {
                var validMoves = gameState.GetMovesAllowed(pieceBoard, pieceIndex);
                while (validMoves != 0)
                {
                    ulong bit = validMoves & (~(validMoves - 1)); // Isolate the rightmost set bit
                    int toIndex = BitOps.BitScan(bit); // Get the index of the isolated bit
                    if ((bit & gameState.PlayerStates[1 - TurnIndex].OccupancyBoard) != 0)
                    {
                        // Prioritize capturing moves
                        moves.Insert(0, new Vector2Int(pieceIndex, toIndex));
                    }
                    else
                    {
                        moves.Add(new Vector2Int(pieceIndex, toIndex));
                    }
                    validMoves ^= bit; // Clear the rightmost set bit to continue
                }
            }
        }

        return moves;
    }























public class MinimaxNode
{
    public GameState GameState;   // The current game state
    public int Depth;             // Current depth in the search tree
    public float Alpha;           // Alpha value for pruning
    public float Beta;            // Beta value for pruning
    public bool MaximizingPlayer; // True if the current player is the maximizing player
    public float BestScore;       // Best score found so far at this node
    public List<Vector2Int> Moves; // List of moves to explore at this node
    public int PlayerIndex;

    public MinimaxNode(GameState gameState, int depth, float alpha, float beta, bool maximizingPlayer, int playerIndex)
    {
        GameState = gameState;
        Depth = depth;
        Alpha = alpha;
        Beta = beta;
        MaximizingPlayer = maximizingPlayer;
        BestScore = maximizingPlayer ? int.MinValue : int.MaxValue;
        PlayerIndex = playerIndex;
        Moves = BotState.GenerateAllMoves(gameState, PlayerIndex);  // Populate moves for the current player
    }
}


private float IterativeMinimax(GameState gameState, int maxDepth, float alpha, float beta, bool maximizingPlayer, int playerIndex)
{
    //string hashKey = gameState.HashA(); // Generate the hash for the current game state

    Stack<MinimaxNode> stack = new Stack<MinimaxNode>();  // Stack for the iterative search
    //int playerIndex = maximizingPlayer ? TurnIndex : 1 - TurnIndex;  // Set the player index

    stack.Push(new MinimaxNode(gameState, 0, alpha, beta, maximizingPlayer, playerIndex));  // Push the initial node

    float bestScore = maximizingPlayer ? int.MinValue : int.MaxValue;

    while (stack.Count > 0)
    {
        MinimaxNode currentNode = stack.Peek();  // Look at the top node on the stack

        // Check for cached evaluation
        string hashKey = currentNode.GameState.HashA();
        if (TT.TryGetValue(hashKey, out float cachedValue))
        {
            // If cached, pop the current node, and continue with the next node
            stack.Pop();
            currentNode.BestScore = cachedValue;
            continue;
        }

        // If we reached the maximum depth or a terminal state, evaluate
        if (currentNode.Depth == maxDepth || IsGameOver(currentNode.GameState))
        {
            // Evaluate the state at the current depth
            currentNode.BestScore = EvaluateGameState(currentNode.GameState);
            TT[hashKey] = currentNode.BestScore;  // Store in transposition table
            stack.Pop();  // Pop the node after evaluation
        }
        else
        {
            // Explore the next move at this depth
            if (currentNode.Moves.Count == 0)
            {
                // No more moves to explore at this node; backtrack
                stack.Pop();
            }
            else
            {
                // Pick the next move to explore
                Vector2Int move = currentNode.Moves[0];  // Get the first move (you can adjust this based on your move ordering strategy)
                currentNode.Moves.RemoveAt(0);  // Remove the move from the list

                GameState clonedGame = currentNode.GameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);  // Apply the move

                // Create a new node for the next depth level
                MinimaxNode newNode = new MinimaxNode(clonedGame, currentNode.Depth + 1, currentNode.Alpha, currentNode.Beta, !currentNode.MaximizingPlayer, 1-currentNode.PlayerIndex);
                stack.Push(newNode);  // Push the new node onto the stack
            }

            // After exploring a move, perform Alpha-Beta pruning
            if (currentNode.MaximizingPlayer)
            {
                bestScore = Mathf.Max(bestScore, currentNode.BestScore);
                currentNode.Alpha = Mathf.Max(currentNode.Alpha, currentNode.BestScore);

                // Alpha-Beta pruning
                if (currentNode.Beta <= currentNode.Alpha)
                    stack.Pop();  // Pop the current node if pruning occurs
            }
            else
            {
                bestScore = Mathf.Min(bestScore, currentNode.BestScore);
                currentNode.Beta = Mathf.Min(currentNode.Beta, currentNode.BestScore);

                // Alpha-Beta pruning
                if (currentNode.Beta <= currentNode.Alpha)
                    stack.Pop();  // Pop the current node if pruning occurs
            }
        }
    }

    return bestScore;
}


}




/*
depth = 4
4:00
42
5:23
1:48
4:42
3:45
1:18
3:04
3:18
2:49
2:46
3:02
*/


/*
depth of 6
55
1:20
4:37
1:07
12:16
--ended too slow to be feasible

depth of 6 no move ordering
1:10
1:28
2:00
0:30
8:43

depth 4
41
19
1:40
3:14
3:18
3:10

*/
