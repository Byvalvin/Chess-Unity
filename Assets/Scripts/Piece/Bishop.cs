using System.Collections.Generic;
using UnityEngine;

public class BishopState : PieceState
{
    public BishopState(bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint) : base(_colour, _currentPos,  _minPoint, _maxPoint)
    {
        this.type = "Bishop";
    }

    // Copy constructor
    public BishopState(BishopState original) : base(original) { }

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        // Check if the move is diagonal
        int deltaX = Mathf.Abs(currentPos.x - to.x);
        int deltaY = Mathf.Abs(currentPos.y - to.y);

        return deltaX == deltaY; // Diagonal moves only
    }

    protected override void SetValidMoves()
    {
        validMoves.Clear(); // Clear previous moves
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();

        int x = currentPos.x;
        int y = currentPos.y;

        // Directions: top-left, bottom-left, bottom-right, top-right
        Vector2Int[] directions = {
            new Vector2Int(-1, 1),   // top-left
            new Vector2Int(-1, -1),  // bottom-left
            new Vector2Int(1, -1),   // bottom-right
            new Vector2Int(1, 1)     // top-right
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

    public override PieceState Clone() => new BishopState(this);
}

public class Bishop : Piece
{
    public override void Move(Vector2Int to)
    {
        if (state.CanMove(to)) // Check if the move is valid
        {
            state.Move(to); // Update the state
            SetPosition(); // Update visual position
        }
    }

    protected override void Awake()
    {
        base.Awake();
        state = new BishopState(); // Initialize bishop state
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
