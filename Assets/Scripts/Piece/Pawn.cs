using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private bool canBeCapturedEnPassant = false;
    private int enPassantCounter = 0;

    public bool CanBeCapturedEnPassant => canBeCapturedEnPassant;
   

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        int forwardStep = colour ? -1 : 1;
        bool sameX = currentPos.x == to.x;
        bool forwardMove = currentPos.y + forwardStep == to.y;
        bool doubleForwardMove = FirstMove && currentPos.y + 2 * forwardStep == to.y;
        bool diagonalCapture = forwardMove && Mathf.Abs(currentPos.x - to.x) == 1;

        return (forwardMove && sameX) || (doubleForwardMove && sameX) || diagonalCapture;
    }

    public override void Move(Vector2Int to) // just used tp update state since move check done on board, make sure to call this base.Move() in sub classes if it is being overidden
    {
        
        int forwardStep = colour ? -1 : 1;
        bool doubleForwardMove = FirstMove && currentPos.y + 2 * forwardStep == to.y;
        if (doubleForwardMove)
        {
            canBeCapturedEnPassant = true; // Set en passant available
            enPassantCounter++;
        }

        base.Move(to);
    }
    
    protected override void SetValidMoves()
    {
        //Debug.Log("My current pos: " + currentPos);
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>
        {
            new Vector2Int( currentPos.x, currentPos.y + (colour ? -1 : 1) ) // One space forward
        };

        if (FirstMove)
        {
            moves.Add(new Vector2Int( currentPos.x, currentPos.y + (colour ? -2 : 2) )); // Two spaces forward
            canBeCapturedEnPassant = true; //potential en passant
        }

        // Diagonal captures
        moves.Add(new Vector2Int( currentPos.x - 1, currentPos.y + (colour ? -1 : 1)) );
        moves.Add(new Vector2Int( currentPos.x + 1, currentPos.y + (colour ? -1 : 1)) );

        // Filter valid moves using HashSet
        validMoves = FindAll(moves);
    }

    public void ResetEnPassant()
    {
        if(enPassantCounter>=2)
            canBeCapturedEnPassant = false; // Reset after one move cycle = 2 moves between both players
        else
            enPassantCounter++;
    }

    // GUI
    public override void HandleInput()
    {
        // Placeholder for input handling
    }

    protected override void Awake(){
        base.Awake();
        type = "Pawn";
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
