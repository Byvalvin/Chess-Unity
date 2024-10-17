using System.Collections.Generic;
using UnityEngine;
using System; // math
using System.Threading.Tasks; // pARALLEL
using System.Threading; // iNterlocked
using System.Collections.Concurrent; // concurrent bags
using System.Linq; // Max, Min



/*
Strategies for Optimization
- Move Ordering: Prioritize captures and checks to enhance pruning effectiveness.
- Transposition Tables: Store results of previously computed positions.
- Parallel Processing: Utilize if applicable for move evaluations.
- Incremental Search: Build on previous searches for efficiency.
*/

public class LeviState : BotState
{
    private const int MaxDepth = 3;
    private const int KingThreatPenalty = 10;
    private const int CaptureScoreMultiplier = 2;
    private const int PieceProtectionReward = 5;

    public LeviState(string playerName, bool colour) : base(playerName, colour) { }
    public LeviState(LeviState original) : base(original) { }
    public override PlayerState Clone() => new LeviState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone){
        clone.MakeBotMove(from, to);
        return Minimax(clone, MaxDepth, int.MinValue, int.MaxValue, Colour);
    }

    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer){
        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);
        
        var moves = GenerateAllMoves(gameState, maximizingPlayer ? TurnIndex : 1 - TurnIndex);
        var results = new ConcurrentBag<int>();

        Parallel.ForEach(moves, move =>{
            GameState clonedGame = gameState.Clone();
            clonedGame.MakeBotMove(move[0], move[1]);
            int eval = Minimax(clonedGame, depth - 1, alpha, beta, !maximizingPlayer);
            results.Add(eval);

            // Update alpha/beta values outside of this thread
            if (maximizingPlayer)
                alpha = Math.Max(alpha, eval);
            else
                beta = Math.Min(beta, eval);

            // Alpha-beta pruning
            if (beta <= alpha)
                return; // Exit from this iteration early
            
        });

        return maximizingPlayer ? results.Max() : results.Min();
    }



    private bool IsGameOver(GameState gameState){
        // Implement logic to determine if the game is over (checkmate, stalemate, etc.)
        return gameState.IsGameEnd(); // Placeholder
    }
    private List<Vector2Int[]> GenerateAllMoves(GameState gameState, int playerIndex){
        var moves = new ConcurrentBag<Vector2Int[]>();
        var pieces = gameState.PlayerStates[playerIndex].PieceStates;

        // Use a ConcurrentBag for capture moves
        var captureMoves = new ConcurrentBag<Vector2Int[]>();

        Parallel.ForEach(pieces, piece =>{
            var validMoves = gameState.GetMovesAllowed(piece);
            foreach (var to in validMoves){
                PieceState targetPiece = gameState.GetTile(to).pieceState;

                if (targetPiece != null && targetPiece is not KingState && targetPiece.Colour != gameState.PlayerStates[playerIndex].Colour){
                    // Collect capture moves separately
                    captureMoves.Add(new Vector2Int[] { piece.Position, to });
                }
                else{
                    moves.Add(new Vector2Int[] { piece.Position, to });
                }
            }
        });

        // Add capture moves to the main moves bag first
        foreach (var captureMove in captureMoves)
            moves.Add(captureMove);

        return moves.ToList(); // Convert ConcurrentBag to List
    }



    private int EvaluateGameState(GameState gameState){
        int score = 0;
        // game ending moves
        score = GameEndingMove(score, gameState);
        if(score!=0) return score;

        // Evaluate material balance
        score += EvaluateMaterial(gameState);

        // Use Parallel.Invoke for concurrent evaluations
        int positionScore = 0;
        int safetyScore = 0;
        int mobilityScore = 0;
        Parallel.Invoke(
            () => positionScore = EvaluatePositioning(gameState), // Evaluate piece positioning
            () => safetyScore = EvaluateKingSafety(gameState), // Evaluate king safety and control
            () => mobilityScore = EvaluateMobility(gameState)
        );

        score += positionScore + safetyScore + mobilityScore;

        return score;
    }

    private int EvaluateMaterial(GameState gameState){
        return ArmyValue(gameState, TurnIndex) - ArmyValue(gameState, 1 - TurnIndex);
    }

    private int EvaluatePositioning(GameState gameState){
        // Use local score and aggregate it at the end to avoid locks
        int positionScore = 0;

        Parallel.ForEach(gameState.PlayerStates[TurnIndex].PieceStates, piece =>{
            int score = CentralControlBonus(piece.Position, gameState);
            Interlocked.Add(ref positionScore, score); // Thread-safe addition
        });

        return positionScore;
    }

    private int EvaluateKingSafety(GameState gameState){
        int safetyScore = 0;
        Vector2Int kingPosition = gameState.PlayerStates[TurnIndex].GetKing().Position;
        HashSet<Vector2Int> kingMoves = gameState.PlayerStates[TurnIndex].GetKing().ValidMoves;

        Parallel.ForEach(gameState.PlayerStates[1 - TurnIndex].PieceStates, opponentPiece =>{
            int localScore = 0;

            // Check for direct threats to the king
            if (opponentPiece.ValidMoves.Contains(kingPosition))
            {
                localScore -= KingThreatPenalty * 3; // Higher penalty for being in check
            }

            // Check if opponent can attack king's escape moves
            foreach (var escape in kingMoves){
                if (opponentPiece.ValidMoves.Contains(escape))
                    localScore -= KingThreatPenalty; // Penalty for threatening escape routes
            }

            Interlocked.Add(ref safetyScore, localScore); // Thread-safe addition
        });

        return safetyScore;
    }



    private int EvaluateMobility(GameState gameState){
        int spaceControl = 0;
        int enemyControl = 0;

        // Calculate space control for the current player
        Parallel.ForEach(gameState.PlayerStates[TurnIndex].PieceStates, pieceState => {
            int validMovesCount = pieceState.ValidMoves.Count;
            Interlocked.Add(ref spaceControl, validMovesCount); // Thread-safe addition
        });

        // Calculate enemy control for the opponent
        Parallel.ForEach(gameState.PlayerStates[1 - TurnIndex].PieceStates, pieceState =>{
            int validMovesCount = pieceState.ValidMoves.Count;
            Interlocked.Add(ref enemyControl, validMovesCount); // Thread-safe addition
        });

        return 5 * (spaceControl - enemyControl);
    }

}

public class Levi : Bot
{
    protected override void Awake()
    {
        // Initialize LeviState if needed
        // state = new LeviState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
