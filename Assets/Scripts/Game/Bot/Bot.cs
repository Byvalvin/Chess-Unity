using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BotState : PlayerState
{
    protected static readonly Dictionary<string, int> pieceValue = new Dictionary<string, int>
    {
        { "Pawn", 1 },
        { "Knight", 3 },
        { "Bishop", 3 },
        { "Rook", 5 },
        { "Queen", 9 },
        { "King", int.MaxValue } // King is invaluable
    };
    private const int SafeMovePenalty = 10;
    private const int CentralControlBonusValue = 2;
    private const int KingTileValue = 2;


    public BotState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public BotState(BotState original) : base(original){
        this.currentGame = original.currentGame;
    }
    public override PlayerState Clone() => MemberwiseClone() as BotState;
    public override Vector2Int[] GetMove()
    {
        //Vector2Int moveFrom = new Vector2Int(3,1), moveTo = new Vector2Int(3,3);
        
        Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        foreach (PieceState piece in PieceStates)
        {
            HashSet<Vector2Int> validMoves = CurrentGame.GetMovesAllowed(piece);
            if(validMoves.Count!=0) moveMap[piece.Position] = validMoves;
        }
        
        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        Vector2Int[] completeMove = Evaluate(moveMap);
        Vector2Int moveFrom=completeMove[0], moveTo=completeMove[1];
        return new Vector2Int[]{moveFrom, moveTo};
    }
    protected virtual Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap){
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MinValue;

        var bestMoves = new List<Vector2Int[]>();

        foreach (var kvp in moveMap){
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value){
                int score = EvaluateMove(from, to);
                if (score > bestScore){
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;
                    bestMoves.Clear();
                    bestMoves.Add(new[] { bestFrom, bestTo });
                }
                else if (score == bestScore)
                    bestMoves.Add(new[] { from, to });
                
            }
        }
        return bestMoves.Count > 1 ? bestMoves[Random.Range(0, bestMoves.Count)] : new Vector2Int[] { bestFrom, bestTo };
    }
    protected virtual int EvaluateMove(Vector2Int from, Vector2Int to)=>1; // placeholder assumes all moves are equal but diff bots will have diff scoring

    protected bool InCenter(Vector2Int position)=> (3<=position.x&&position.x<=4 && 3<=position.y&&position.y<=4);

    // some commonon factors to consider
    protected int ArmyValue(GameState gameState, int playerIndex){
        int av = 0;
        foreach (PieceState piece in gameState.PlayerStates[playerIndex].PieceStates)
            if(piece is not KingState)
                av+=pieceValue[piece.Type];
        return av;
    }

    protected int CentralControlBonus(Vector2Int position, GameState gameState)
    {
        // Implement a method to calculate score based on board control
        // Example: add 1 point for controlling the center squares
        int centreControl = InCenter(position)? 2:0;
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates){
            centreControl += Utility.FindAll(piece.validMoves, InCenter).Count;
        }
            
        return centreControl;
    }

    protected int EvaluatePieceSafety(Vector2Int from, Vector2Int to, string type, GameState gameState)
    {
        int toNotSafe = EvaluateSafety(to, gameState, true);
        int fromNotSafe = EvaluateSafety(from, gameState, false);
        return (toNotSafe + fromNotSafe) * pieceValue[type];
    }
    private int EvaluateSafety(Vector2Int position, GameState gameState, bool isTarget)
    {
        int penalty = 0;
        foreach (PieceState opponentPiece in gameState.PlayerStates[1 - TurnIndex].PieceStates)
        {
            if (opponentPiece.ValidMoves.Contains(position))
                penalty -= SafeMovePenalty; // don't go to unsafe positions
        }
        return penalty;
    }

    protected int AttackedKingTiles(GameState nextGame)
    {
        int beforeCount = currentGame.PlayerStates[1 - TurnIndex].GetKing().ValidMoves.Count;
        int afterCount = nextGame.PlayerStates[1 - TurnIndex].GetKing().ValidMoves.Count;
        return afterCount == 0 ? int.MaxValue / 2 : Mathf.Max(0, (beforeCount - afterCount) * 5);
    }

    protected int KingTiles(GameState gameState)
    {
        int tileCount = currentGame.PlayerStates[1 - TurnIndex].GetKing().ValidMoves.Count;
        return tileCount == 0 ? int.MaxValue / 2 : KingTileValue * (8 - tileCount);
    }

    protected int PieceDefended(GameState gameState, PieceState pieceState, Vector2Int to)
    {
        int defendedCount = 0;
        foreach (PieceState allyPiece in gameState.PlayerStates[pieceState.Colour ? 0 : 1].PieceStates)
        {
            if (IsPieceDefending(allyPiece, pieceState, to, gameState))
                defendedCount++;
        }
        return defendedCount;
    }

    private bool IsPieceDefending(PieceState allyPiece, PieceState pieceState, Vector2Int to, GameState gameState)
    {
        switch (allyPiece.Type)
        {
            case "King":
                return gameState.KingAttackedTiles(allyPiece).Contains(to);
            case "Knight":
                return gameState.KnightAttackedTiles(allyPiece).Contains(to);
            case "Pawn":
                return gameState.PawnAttackedTiles(allyPiece).Contains(to);
            default:
                return IsLongRangePieceDefending(allyPiece, pieceState, to, gameState);
        }
    }

    private bool IsLongRangePieceDefending(PieceState allyPiece, PieceState pieceState, Vector2Int to, GameState gameState)
    {
        HashSet<Vector2Int> pointsBetweenAndEnds;

        switch (allyPiece.Type)
        {
            case "Bishop":
                pointsBetweenAndEnds = Utility.GetIntermediateDiagonalLinePoints(allyPiece.Position, to, includeEnds: true);
                break;
            case "Rook":
                pointsBetweenAndEnds = Utility.GetIntermediateNonDiagonalLinePoints(allyPiece.Position, to, includeEnds: true);
                break;
            case "Queen":
                pointsBetweenAndEnds = Utility.GetIntermediateLinePoints(allyPiece.Position, to, includeEnds: true);
                break;
            default:
                return false;
        }

        return pointsBetweenAndEnds.Count > 2 && !HasBlockingPiece(pointsBetweenAndEnds, allyPiece, to, gameState);
    }

    private bool HasBlockingPiece(HashSet<Vector2Int> points, PieceState allyPiece, Vector2Int to, GameState gameState)
    {
        points.Remove(allyPiece.Position);
        points.Remove(to);

        foreach (Vector2Int point in points)
        {
            if (gameState.GetTile(point).HasPieceState())
                return true; // Blocked
        }

        return false; // Not blocked
    }

}
public abstract class Bot : Player
{

    protected override void Awake()
    {
        //state = new BotState();
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}

