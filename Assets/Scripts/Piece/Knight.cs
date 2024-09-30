using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightState : PieceState
{
    public KnightState() : base()
    {
        this.type = "Knight";
    }

    // Copy constructor
    public KnightState(KnightState original) : base(original) { }

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        // Knight movement (L-shape)
        int deltaX = Mathf.Abs(currentPos.x - to.x);
        int deltaY = Mathf.Abs(currentPos.y - to.y);

        return (deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2);
    }

    protected override void SetValidMoves()
    {
        validMoves.Clear(); // Clear previous moves
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();

        // Possible knight moves (L-shaped)
        Vector2Int[] knightMoves = {
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(1, -2),
            new Vector2Int(-1, 2), new Vector2Int(-1, -2)
        };

        foreach (var move in knightMoves)
        {
            Vector2Int targetPos = currentPos + move;
            if (InBounds(targetPos))
            {
                moves.Add(targetPos);
            }
        }

        validMoves = FindAll(moves);
    }

    public override PieceState Clone() => new KnightState(this);
}

public class Knight : Piece
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
        state = new KnightState(); // Initialize knight state
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
