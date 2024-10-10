using System.Collections.Generic;
using UnityEngine;

public class KingState : PieceState
{
    public KingState(bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint) : base(_colour, _currentPos,  _minPoint, _maxPoint)
    {
        this.type = "King";
    }

    // Copy constructor
    public KingState(KingState original) : base(original) { }

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        // Allow moving one square in any direction
        int deltaX = Mathf.Abs(currentPos.x - to.x);
        int deltaY = Mathf.Abs(currentPos.y - to.y);

        // castling
        bool castleMove = firstMove && currentPos.y==to.y && deltaX==2;

        return (deltaX <= 1 && deltaY <= 1) || castleMove; // Valid if the move is within one square
    }

    protected override void SetValidMoves()
    {
        //validMoves.Clear(); // Clear previous moves
        HashSet<Vector2Int> moves = Utility.GetSurroundingPoints(currentPos);
        // add castling
        // conditions
        // havent made first move(will check all other conditions at game)
        HashSet<Vector2Int> castleMoves = new HashSet<Vector2Int>{
            new Vector2Int(currentPos.x-2, currentPos.y),
            new Vector2Int(currentPos.x+2, currentPos.y)
        };
        if(firstMove) moves.UnionWith(castleMoves);
        validMoves = FindAll(moves);
    }

    public override PieceState Clone() => new KingState(this);
}

public class King : Piece
{    


    protected override void Awake()
    {
        base.Awake();
        //state = new KingState(); // Initialize king state
    }
    protected override void Start()
    {
        base.Start();
        SetSprite();
    }
    protected override void Update()
    {
        base.Update();
    }
}
