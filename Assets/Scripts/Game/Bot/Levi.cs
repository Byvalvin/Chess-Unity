using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

trategies for Optimization
Move Ordering:

Implement move ordering to prioritize likely better moves, which enhances pruning effectiveness. Consider checking for captures first, then high-value moves.
Transposition Tables:

Implement transposition tables to store and reuse results of previously computed positions. This can significantly reduce computation time for positions that recur in the search tree.
Parallel Processing:

If applicable, consider using parallel processing to evaluate moves in parallel. However, ensure thread safety when accessing shared resources.
Incremental Search:

If your game allows, consider using an incremental search technique where the bot builds on previous searches instead of starting from scratch.




Move Ordering Enhancements
Prioritize Captures:

Since you're already adding a bonus for captures in the EvaluateMove method, you can further refine how you order the moves during the evaluation phase. Before evaluating all moves, you can separate capturing moves from non-capturing ones and evaluate captures first.
Categorize Moves:

You can categorize moves into three groups: captures, checks, and non-capturing moves. This way, you can prioritize the most critical moves:
Captures: Evaluate these first, as they often have a significant impact on the game.
Checks: Moves that put the opponent's king in check should be prioritized next, as they force a reaction from your opponent.
Non-capturing moves: Evaluate these last.
Improve Capture Detection:

When generating moves in your GenerateAllMoves method, you can add a check for whether a target square contains an enemy piece. This will allow you to build a list of capturing moves directly, making the ordering process smoother.
*/
public class LeviState : BotState
{
    private const int MaxDepth = 3; // You can adjust this for performance vs. accuracy

    public LeviState(string playerName, bool colour) : base(playerName, colour) { }
    public LeviState(BotState original) : base(original) { }

    protected override Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MinValue;

        Dictionary<int, Vector2Int[]> bestMovesMap = new Dictionary<int, Vector2Int[]>();
        int dupIndex = 0;

        foreach (var kvp in moveMap)
        {
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value)
            {
                GameState clonedGame = currentGame.Clone(); // Clone the current game state
                clonedGame.MakeBotMove(from, to); // Simulate the move

                // Evaluate using Minimax
                int score = Minimax(clonedGame, MaxDepth, int.MinValue, int.MaxValue, Colour);
                Debug.Log("score eval: " + from  + to + score);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;

                    dupIndex = 0;
                    bestMovesMap.Clear();
                    bestMovesMap[dupIndex]=new Vector2Int[]{bestFrom,bestTo};
                    dupIndex++;
                }
                else if(score==bestScore)
                {
                    bestMovesMap[++dupIndex]=new Vector2Int[]{from,to};
                }
            }
        }
        if(dupIndex > 1) // first item is the original so more 2 OR MORE only
        {
            return bestMovesMap[Random.Range(0,dupIndex)];
        }

        return new Vector2Int[] { bestFrom, bestTo };
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsGameOver(gameState))
        {
            return EvaluateGameState(gameState);
        }

        if (maximizingPlayer==Colour)
        {
            int maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex))
            {
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move[0], move[1]); // Make the move
                
                int eval = Minimax(clonedGame, depth - 1, alpha, beta, !Colour);
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

                int eval = Minimax(clonedGame, depth - 1, alpha, beta, Colour);
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
        bool moveOrderPredicate = false; // faster pruning in ordering

        foreach (var piece in pieces)
        {
            var validMoves = gameState.GetMovesAllowed(piece);
            foreach (var to in validMoves)
            {
                // Check if the move captures an enemy piece
                PieceState targetPiece = gameState.GetTile(to).pieceState;
                moveOrderPredicate = targetPiece != null && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour;
                if (moveOrderPredicate)
                {
                    // Prioritize capture moves by adding them first
                    moves.Insert(0, new Vector2Int[] { piece.Position, to }); // Insert at the start
                }
                else
                {
                    // Add non-capturing moves normally
                    moves.Add(new Vector2Int[] { piece.Position, to });
                }
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
        //Central control
        score += EvaluatePositioning(gameState);

        // Evaluate king safety

        // Additional heuristics can be added here
        // e.g., control of center, piece activity, etc.

        
        // 3. Mobility, contrrol
        // find a move that increases the number of valid moves a piece the most(to increase the chance to capture)
        int spaceControl = 0, enemyControl = 0;
        foreach (PieceState pieceState in gameState.PlayerStates[TurnIndex].PieceStates)
            spaceControl += pieceState.ValidMoves.Count;
        foreach (PieceState pieceState in gameState.PlayerStates[1-TurnIndex].PieceStates)
            enemyControl += pieceState.ValidMoves.Count;
        score += 5*(spaceControl-enemyControl); // score is now more relative to the enemy

        // 4. Piece Saftety
        foreach (PieceState mypiece in gameState.PlayerStates[TurnIndex].PieceStates)
            foreach (PieceState opponentPiece in gameState.PlayerStates[1-TurnIndex].PieceStates)
                if(opponentPiece.ValidMoves.Contains(mypiece.Position)){
                    score += (PieceDefended(gameState, mypiece, mypiece.Position)*pieceValue[mypiece.Type] - 20); // if piece is defended more than it is attacked them move is good
        }


        // 6. King attacks
        score += 5*KingTiles(gameState);

        return score;
    }

    private int EvaluateMaterial(GameState gameState) => ArmyValue(gameState, TurnIndex) - ArmyValue(gameState, 1-TurnIndex);

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
