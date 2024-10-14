using System.Collections.Generic;
using UnityEngine;


public class PawnState : PieceState{
    private bool canBeCapturedEnPassant, promoted;
    private int enPassantCounter;
    public bool CanBeCapturedEnPassant => canBeCapturedEnPassant;
    public bool Promoted {
        get=>promoted;
        set{
            promoted=value;
            if(promoted)Position=heavenOrhell;
        }
    }

    public PawnState(bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint) : base(_colour, _currentPos,  _minPoint, _maxPoint){
        this.type = "Pawn";
        this.canBeCapturedEnPassant = false;
        this.promoted = false;
        this.enPassantCounter = 0;
    }

    // Copy constructor
    public PawnState(PawnState original) : base(original){
        canBeCapturedEnPassant = original.canBeCapturedEnPassant;
        enPassantCounter = original.enPassantCounter;
        promoted = original.promoted;
    }
    
    public override bool CanMove(Vector2Int to){
        if (!InBounds(to)) return false;

        int forwardStep = colour ? -1 : 1;
        bool sameX = currentPos.x == to.x;
        bool forwardMove = currentPos.y + forwardStep == to.y;
        bool doubleForwardMove = firstMove && currentPos.y + 2 * forwardStep == to.y;
        bool diagonalCapture = forwardMove && Mathf.Abs(currentPos.x - to.x) == 1;

        return (forwardMove && sameX) || (doubleForwardMove && sameX) || diagonalCapture;
    }

    protected override void SetValidMoves()
    {
        //validMoves.Clear(); // Clear previous moves
        int forwardStep = colour ? -1 : 1;
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>
        {
            new Vector2Int(currentPos.x, currentPos.y + forwardStep) // One space forward
        };

        if (firstMove)
        {
            moves.Add(new Vector2Int(currentPos.x, currentPos.y + 2*forwardStep)); // Two spaces forward
        }

        // Diagonal captures
        moves.Add(new Vector2Int(currentPos.x - 1, currentPos.y + forwardStep));
        moves.Add(new Vector2Int(currentPos.x + 1, currentPos.y + forwardStep));

        validMoves = FindAll(moves);
    }
    public override void Move(Vector2Int to) // just used tp update state since move check done on board, make sure to call this base.Move() in sub classes if it is being overidden
    {
        
        int forwardStep = colour ? -1 : 1;
        bool doubleForwardMove = FirstMove && currentPos.y + 2 * forwardStep == to.y;
        if (doubleForwardMove){
            canBeCapturedEnPassant = true; // Set en passant available
            //enPassantCounter++; // Type-Safe Casting: When accessing PawnState specific properties, you can cast the state to PawnState.
        }

        base.Move(to);
    }

    public override PieceState Clone() => new PawnState(this);
    

    public void ResetEnPassant(){
        if (canBeCapturedEnPassant){
            if (enPassantCounter >= 2)
                canBeCapturedEnPassant = false; // Reset after one move cycle
            else
                enPassantCounter++;
        }
    }
}


public class Pawn : Piece{
    // public override void Move(Vector2Int to)
    // {
    //     state.Move(to); // Update the state
    //     SetPosition(); // Update visual position
    // }

    // GUI
    protected override void Awake(){
        //state = new PawnState();
        base.Awake();
    }
    protected override void Start(){
        base.Start();
        SetSprite();
    }

    protected override void Update(){
        base.Update();
    }
}
