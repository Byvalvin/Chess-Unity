using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private bool firstMove = true;

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        bool sameX = currentPos.x == to.x,
            forwardMove = (colour && currentPos.y - 1 == to.y) || (!colour && currentPos.y + 1 == to.y),
            doubleForwardMove = firstMove && ((colour && currentPos.y - 2 == to.y) || (!colour && currentPos.y + 2 == to.y)),
            diagonalCapture = forwardMove && Mathf.Abs(currentPos.x - to.x) == 1;

        return (forwardMove && sameX) || (doubleForwardMove && sameX) || diagonalCapture;
    }
    public override void Move(Vector2Int to)
    {

        currentPos = to;
        firstMove = false; // Mark as moved
        SetPosition();
    }

    public override void SetValidMoves(){
        Debug.Log("My current pos"+ currentPos);
        List<Vector2Int> moves = new List<Vector2Int>
        {
            new Vector2Int(currentPos.x, colour ? currentPos.y - 1 : currentPos.y + 1) // One space forward
        };

        if (firstMove)
            moves.Add(new Vector2Int(currentPos.x, colour ? currentPos.y - 2 : currentPos.y + 2)); // Two spaces forward

        // Diagonal captures
        moves.Add(new Vector2Int(currentPos.x - 1, colour ? currentPos.y - 1 : currentPos.y + 1));
        moves.Add(new Vector2Int(currentPos.x + 1, colour ? currentPos.y - 1 : currentPos.y + 1));

        validMoves = moves.FindAll(CanMove); // Filter valid moves
    }


    // GUI
    public override void HandleInput()
    {

    }


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        type = "Pawn";

        SetPosition();

        // Load sprite
        //pieceSprite = Resources.Load<Sprite>("Pawn");
     
        SetSprite();   
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
    }
}
