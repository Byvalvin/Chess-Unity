using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BotState : PlayerState
{
    protected static Dictionary<string, int> pieceValue = new Dictionary<string, int>
    {
        { "Pawn", 1 },
        { "Knight", 3 },
        { "Bishop", 3 },
        { "Rook", 5 },
        { "Queen", 9 },
        { "King", int.MaxValue } // King is invaluable
    };


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
    protected virtual Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MinValue; // -2 147 483 648
        Dictionary<int, Vector2Int[]> bestMovesMap = new Dictionary<int, Vector2Int[]>();
        int dupIndex = 0;
        

        foreach (var kvp in moveMap)
        {
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value)
            {
                int score = EvaluateMove(from, to);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;

                    dupIndex = 0;
                    bestMovesMap.Clear();
                    bestMovesMap[dupIndex]=new Vector2Int[]{bestFrom,bestTo};
                }
                else if(score==bestScore)
                {
                    bestMovesMap[++dupIndex]=new Vector2Int[]{from,to};
                }
            }
        }
        if(dupIndex > 1)
        {
            return bestMovesMap[Random.Range(0,dupIndex)];
        }


        return new Vector2Int[] { bestFrom, bestTo };
    }
    protected virtual int EvaluateMove(Vector2Int from, Vector2Int to)=>1; // placeholder assumes all moves are equal but diff bots will have diff scoring

    protected bool InCenter(Vector2Int position){
        HashSet<int> centerCoords = new HashSet<int>{3,4};
        return centerCoords.Contains(position.x) && centerCoords.Contains(position.y);
    }

    // some commonon factors to consider
    protected int ArmyValue(GameState gameState,bool myArmy=true){
        int av = 0;
        foreach (PieceState piece in gameState.PlayerStates[myArmy ? TurnIndex : 1-TurnIndex].PieceStates)
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

        int toNotSafe = 0, fromNotSafe = 0; // must move pieces under attack, dont go to places under attack
        foreach (PieceState opponentPiece in gameState.PlayerStates[1-TurnIndex].PieceStates)
            if(opponentPiece.ValidMoves.Contains(to))
                toNotSafe-=10; // dont go there
        foreach (PieceState opponentPiece in currentGame.PlayerStates[1-TurnIndex].PieceStates)
            if(opponentPiece.ValidMoves.Contains(from))
                fromNotSafe+=10; // dont stay here
        

        // Factor in piece value for safety evaluation
        return (toNotSafe + fromNotSafe)*pieceValue[type]; // Higher penalty for more valuable pieces
    }

    protected int AttackedKingTiles(GameState nextGame){
        int before = currentGame.PlayerStates[1-TurnIndex].GetKing().ValidMoves.Count,
            after = nextGame.PlayerStates[1-TurnIndex].GetKing().ValidMoves.Count,
            difference = before - after;
        return after==0 ? int.MaxValue/2 : difference > 0 ? 5*difference : 0;
    }

    protected int PieceDefended(GameState gameState, PieceState pieceState){
        int defended = 0;
        foreach (PieceState piece in gameState.PlayerStates[pieceState.Colour?0:1].PieceStates)
        {
            switch(piece.Type){
                case "King":
                    if(gameState.KingAttackedTiles(piece).Contains(pieceState.Position))
                        defended++;
                    break;
                case "Knight":
                    if(gameState.KnightAttackedTiles(piece).Contains(pieceState.Position))
                        defended++;
                    break;
                case "Pawn":
                    if(gameState.PawnAttackedTiles(piece).Contains(pieceState.Position))
                        defended++;
                    break;
                default: // Queen, Rook, Bishop
                    HashSet<Vector2Int> pointsBetweenAndEnds;
                    switch(piece.Type){
                        case "Bishop":
                            pointsBetweenAndEnds = Utility.GetIntermediateDiagonalLinePoints(piece.Position, pieceState.Position, includeEnds:true);
                            break;
                        case "Rook":
                            pointsBetweenAndEnds = Utility.GetIntermediateNonDiagonalLinePoints(piece.Position, pieceState.Position, includeEnds:true);
                            break;
                        case "Queen":
                            pointsBetweenAndEnds = Utility.GetIntermediateLinePoints(piece.Position, pieceState.Position, includeEnds:true);
                            break;
                        default:
                            pointsBetweenAndEnds = new HashSet<Vector2Int>();
                            break;
                    }
                    bool pieceAtposDefended = pointsBetweenAndEnds.Count != 0; // if set is empty, then opposingPiece is not a defender
                    if(pointsBetweenAndEnds.Count > 2){ // if there is a defender then only do this check if there are tiles between the defender and defended
                        pointsBetweenAndEnds.Remove(piece.Position); pointsBetweenAndEnds.Remove(pieceState.Position);
                        foreach (Vector2Int point in pointsBetweenAndEnds){
                            pieceAtposDefended = pieceAtposDefended && !gameState.GetTile(point).HasPieceState(); // if a single piece on path, path is blocked and piece cant be defended
                            if(!pieceAtposDefended)
                                break; // there is another piece blocking the defense, onto next candidate
                        }
                    }
                    if(pieceAtposDefended)
                        defended++;
                    break;
            }
        }
        return defended;
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

