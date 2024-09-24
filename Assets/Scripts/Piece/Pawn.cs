using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private bool firstMove = true;

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        int forwardStep = colour ? -1 : 1;
        bool sameX = currentPos.x == to.x;
        bool forwardMove = currentPos.y + forwardStep == to.y;
        bool doubleForwardMove = firstMove && currentPos.y + 2 * forwardStep == to.y;
        bool diagonalCapture = forwardMove && Mathf.Abs(currentPos.x - to.x) == 1;

        return (forwardMove && sameX) || (doubleForwardMove && sameX) || diagonalCapture;
    }

    public override void Move(Vector2Int to)
    {
        Position = to;
        firstMove = false; // Mark as moved
    }

    protected override void SetValidMoves()
    {
        //Debug.Log("My current pos: " + currentPos);
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>
        {
            new Vector2Int( currentPos.x, currentPos.y + (colour ? -1 : 1) ) // One space forward
        };

        if (firstMove)
            moves.Add(new Vector2Int( currentPos.x, currentPos.y + (colour ? -2 : 2) )); // Two spaces forward

        // Diagonal captures
        moves.Add(new Vector2Int( currentPos.x - 1, currentPos.y + (colour ? -1 : 1)) );
        moves.Add(new Vector2Int( currentPos.x + 1, currentPos.y + (colour ? -1 : 1)) );

        // Filter valid moves using HashSet
        validMoves = new HashSet<Vector2Int>();
        foreach (var move in moves)
        {
            if (CanMove(move))
                validMoves.Add(move);
        }
    }

    // GUI
    public override void HandleInput()
    {
        // Placeholder for input handling
    }

    protected override void Start()
    {
        base.Start();
        type = "Pawn";
        SetPosition();
        SetSprite();   
    }

    protected override void Update()
    {
        base.Update();
    }
}
