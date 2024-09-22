using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private bool firstMove = true;

    public override bool CanMove(Vector2 to)
    {
        if (!InBounds(to)) return false;

        bool sameX = currentPos.x == to.x;
        bool forwardMove = (colour && currentPos.y - 1 == to.y) || (!colour && currentPos.y + 1 == to.y);
        bool doubleForwardMove = firstMove && ((colour && currentPos.y - 2 == to.y) || (!colour && currentPos.y + 2 == to.y));
        bool diagonalCapture = forwardMove && Mathf.Abs(currentPos.x - to.x) == 1;

        return (forwardMove && sameX) || doubleForwardMove || diagonalCapture;
    }
    public override void Move(Vector2 to)
    {
        if (CanMove(to))
        {
            currentPos = to;
            firstMove = false; // Mark as moved
            SetPosition();
        }
    }

    public override List<Vector2> GetValidMoves(){
        Debug.Log("My current pos"+ currentPos);
        List<Vector2> validMoves = new List<Vector2>
        {
            new Vector2(currentPos.x, colour ? currentPos.y - 1 : currentPos.y + 1) // One space forward
        };

        if (firstMove)
            validMoves.Add(new Vector2(currentPos.x, colour ? currentPos.y - 2 : currentPos.y + 2)); // Two spaces forward

        // Diagonal captures
        validMoves.Add(new Vector2(currentPos.x - 1, colour ? currentPos.y - 1 : currentPos.y + 1));
        validMoves.Add(new Vector2(currentPos.x + 1, colour ? currentPos.y - 1 : currentPos.y + 1));

        return validMoves.FindAll(CanMove); // Filter valid moves

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
