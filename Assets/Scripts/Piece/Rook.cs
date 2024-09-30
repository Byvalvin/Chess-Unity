using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookState : PieceState
{
    public RookState() : base()
    {
        this.type = "Rook";
    }

    // Copy constructor
    public RookState(RookState original) : base(original) { }

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        // Rook can move horizontally or vertically
        return currentPos.x == to.x || currentPos.y == to.y;
    }

    protected override void SetValidMoves()
    {
        validMoves.Clear(); // Clear previous moves
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();

        int x = currentPos.x;
        int y = currentPos.y;

        // Directions: horizontal and vertical
        Vector2Int[] directions = {
            new Vector2Int(1, 0),  // right
            new Vector2Int(-1, 0), // left
            new Vector2Int(0, 1),  // up
            new Vector2Int(0, -1)  // down
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

    public override PieceState Clone() => new RookState(this);
}

public class Rook : Piece
{
    public override void Move(Vector2Int to)
    {
        if (state.CanMove(to))
        {
            state.Move(to);
            SetPosition(); // Update visual position
        }
    }
    protected override void Awake()
    {
        state = new RookState(); // Initialize rook state
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
