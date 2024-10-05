using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenState : PieceState
{
    public QueenState(bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint) : base(_colour, _currentPos,  _minPoint, _maxPoint)
    {
        this.type = "Queen";
    }

    // Copy constructor
    public QueenState(QueenState original) : base(original) { }

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        int deltaX = Mathf.Abs(currentPos.x - to.x);
        int deltaY = Mathf.Abs(currentPos.y - to.y);

        // Queen can move diagonally or straight
        return deltaX == deltaY || currentPos.x == to.x || currentPos.y == to.y;
    }

    protected override void SetValidMoves()
    {
        validMoves.Clear(); // Clear previous moves
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();

        int x = currentPos.x;
        int y = currentPos.y;

        // Directions: horizontal, vertical, and diagonal
        Vector2Int[] directions = {
            new Vector2Int(1, 0),  // right
            new Vector2Int(-1, 0), // left
            new Vector2Int(0, 1),  // up
            new Vector2Int(0, -1), // down
            new Vector2Int(1, 1),  // up-right
            new Vector2Int(-1, -1),// down-left
            new Vector2Int(1, -1), // down-right
            new Vector2Int(-1, 1)  // up-left
        };

        foreach (var direction in directions)
        {
            int tX = x, tY = y;

            while (true)
            {
                tX += direction.x;
                tY += direction.y;

                if (!InBounds(new Vector2Int(tX, tY))) break;

                moves.Add(new Vector2Int(tX, tY));
            }
        }

        validMoves = FindAll(moves);
    }
    public override PieceState Clone() => new QueenState(this);
}

public class Queen : Piece
{
    protected override void Awake()
    {
        //state = new QueenState(); // Initialize queen state
        base.Awake();
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
