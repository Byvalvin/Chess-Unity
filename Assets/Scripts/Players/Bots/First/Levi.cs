using System;
using System.Collections.Generic;
using UnityEngine;

public class MinimaxNode
{
    public GameState gameState;  // The game state at this node
    public int depth;            // The current depth in the search tree
    public bool maximizingPlayer; // Whether this is a maximizing or minimizing player's turn
    public float value;          // The evaluation value for this node
    public int childrenEvaluated; // The number of child nodes explored

    public MinimaxNode(GameState gameState, int depth, bool maximizingPlayer)
    {
        this.gameState = gameState;
        this.depth = depth;
        this.maximizingPlayer = maximizingPlayer;
        this.value = maximizingPlayer ? int.MinValue : int.MaxValue;
        this.childrenEvaluated = 0;
    }
}

public class Levi : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class LeviState : BotState
{
    private const int MaxDepth = 6,
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
        //float moveScore = IterativeMinimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite);
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
        //score = GameEndingMove(score, gameState);
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
        return EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex);
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


















    
    // Iterative Minimax with Transposition Table (TT) caching and alpha-beta pruning
    private float IterativeMinimax(GameState gameState, int maxDepth, float alpha, float beta, bool maximizingPlayer)
    {
        Stack<MinimaxNode> stack = new Stack<MinimaxNode>();
        MinimaxNode rootNode = new MinimaxNode(gameState, 0, maximizingPlayer);
        stack.Push(rootNode);

        // Iteratively process each node in the tree
        while (stack.Count > 0)
        {
            MinimaxNode currentNode = stack.Peek();

            // If we're at a leaf node (max depth or game over), evaluate and backtrack
            if (currentNode.depth == maxDepth || IsGameOver(currentNode.gameState))
            {
                // Get hash key for transposition table
                string hashKey = currentNode.gameState.HashA(); 

                // Check if the game state is already evaluated and stored in the TT
                if (TT.TryGetValue(hashKey, out float cachedValue))
                {
                    currentNode.value = cachedValue; // Use cached value from TT
                }
                else
                {
                    // If not cached, evaluate the node and store the result in the TT
                    currentNode.value = EvaluateGameState(currentNode.gameState);
                    TT[hashKey] = currentNode.value; // Store evaluation in TT
                }

                // Backtrack (pop the node)
                stack.Pop();
            }
            else
            {
                // Generate child nodes (possible moves)
                List<Vector2Int> moves = GenerateAllMoves(currentNode.gameState, currentNode.maximizingPlayer ? TurnIndex : 1 - TurnIndex);

                // If there are still moves to explore
                if (currentNode.childrenEvaluated < moves.Count)
                {
                    // Generate the next child node to explore
                    Vector2Int move = moves[currentNode.childrenEvaluated];
                    GameState clonedGame = currentNode.gameState.Clone();  // Clone the game state
                    clonedGame.MakeBotMove(move.x, move.y);
                    MinimaxNode childNode = new MinimaxNode(clonedGame, currentNode.depth + 1, !currentNode.maximizingPlayer);
                    stack.Push(childNode);

                    // Increment the number of children evaluated for this node
                    currentNode.childrenEvaluated++;
                }
                else
                {
                    // After processing all children, perform the evaluation and backtrack
                    if (currentNode.maximizingPlayer)
                    {
                        // Maximizing player: we want the maximum score
                        float maxEval = int.MinValue;

                        // Evaluate the current node's value based on its children
                        foreach (MinimaxNode child in stack)
                        {
                            maxEval = Mathf.Max(maxEval, child.value);
                        }

                        currentNode.value = maxEval;
                        alpha = Mathf.Max(alpha, maxEval);

                        // Alpha-beta pruning
                        if (beta <= alpha)
                        {
                            stack.Pop(); // Backtrack
                            continue;
                        }
                    }
                    else
                    {
                        // Minimizing player: we want the minimum score
                        float minEval = int.MaxValue;

                        // Evaluate the current node's value based on its children
                        foreach (MinimaxNode child in stack)
                        {
                            minEval = Mathf.Min(minEval, child.value);
                        }

                        currentNode.value = minEval;
                        beta = Mathf.Min(beta, minEval);

                        // Alpha-beta pruning
                        if (beta <= alpha)
                        {
                            stack.Pop(); // Backtrack
                            continue;
                        }
                    }

                    // Store the evaluation in the transposition table
                    string hashKey = currentNode.gameState.HashA();
                    TT[hashKey] = currentNode.value;

                    // Backtrack (pop the node)
                    stack.Pop();
                }
            }
        }

        // Return the evaluation of the root node
        return rootNode.value;
    }
}






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



*/
