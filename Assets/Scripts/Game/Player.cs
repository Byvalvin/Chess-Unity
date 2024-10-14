using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerState{
    public event Action<PieceState> OnPieceRemoved, OnPieceCaptured;
    private string playerName;
    private bool colour = true; //assume white
    private int turnIndex = 0; //assume white turn
    private float tileSize;
    private List<PieceState> pieces = new List<PieceState>(), captured = new List<PieceState>();

    private bool inCheck = true, doubleCheck = true;

    public PieceState KingAttacker = null; // the opposing piece attacking player's king 

    public string PlayerName{
        get=>playerName;
        set=>playerName=value;
    }
    public bool Colour{
        get=>colour;
        set{
            colour=value;
            turnIndex=colour?0:1;
        }
    }
    protected int TurnIndex => turnIndex;
    public float TileSize{
        get=>tileSize;
        set=>tileSize=value;
    }
    public List<PieceState> PieceStates=>pieces;
    public List<PieceState> CappedStates=>captured;

    public bool InCheck{
        get=>inCheck;
        set=>inCheck=value;
    }
    public bool DoubleCheck{
        get{return doubleCheck;}
        set=>doubleCheck=value;
    }

    public PlayerState(string _playerName, bool _colour){
        playerName = _playerName; Colour=_colour;
    }

    public PlayerState(PlayerState original){
        this.playerName = original.playerName;
        this.colour = original.colour;
        this.turnIndex = original.turnIndex;
        this.tileSize = original.tileSize;
        this.pieces = new List<PieceState>();
        foreach (var piece in original.pieces)
            pieces.Add(piece.Clone()); // Assuming PieceState has a Clone method
        this.captured = new List<PieceState>();
        foreach (var piece in original.captured)
            captured.Add(piece.Clone());
        

        this.inCheck = original.inCheck;
        this.doubleCheck = original.doubleCheck;
        this.KingAttacker = original.KingAttacker?.Clone(); // Handle cloning if necessary
    }

    public virtual PlayerState Clone() => new PlayerState(this); 

    public PieceState GetKing() => PieceStates[0];
    public bool IsInCheck()=>doubleCheck || InCheck;
    public void AddPieceState(PieceState piece) => pieces.Add(piece);
    public void RemovePieceState(PieceState piece) {
        pieces.Remove(piece);
        OnPieceRemoved?.Invoke(piece); // Notify listeners
    }

    public void Capture(PieceState piece) {
        captured.Add(piece);
        OnPieceCaptured?.Invoke(piece); // Notify listeners
    }


    public virtual Vector2Int[] GetMove(){
        Vector2Int[] fromTo = new Vector2Int[2];
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / tileSize);
        fromTo[1] = targetPosition;
        return fromTo;
    }

    //bots
    protected GameState currentGame;
    public GameState CurrentGame{
        get=>currentGame;
        set=>currentGame=value;
    }

}

public class Player : MonoBehaviour{
   protected PlayerState state;
   protected List<Piece> pieces = new List<Piece>(), capturedPieces = new List<Piece>();

   public PlayerState State{
       get=>state;
       set{
           if (state != null){// Unsubscribe from previous state events
                state.OnPieceRemoved -= RemovePiece;
                state.OnPieceCaptured -= Capture;
            }
            state = value;
            // Subscribe to events of the new state
            state.OnPieceRemoved += RemovePiece;
            state.OnPieceCaptured += Capture;
       }
   }

   public List<Piece> Pieces => pieces;
   public List<Piece> CapturedPieces => capturedPieces;


   // need to have the PieceObjects fro dispaly
    public void AddPiece(Piece piece) => pieces.Add(piece);
    public void RemovePiece(PieceState pieceState) {
        // Find the corresponding Piece
        var correspondingPiece = pieces.Find(piece => piece.State == pieceState);
        if (correspondingPiece != null)
            pieces.Remove(correspondingPiece);
    }
    public void Capture(PieceState pieceState){
        // Find corresponding Piece and add to captured pieces
        var correspondingPiece = pieces.Find(piece => piece.State == pieceState);
        if (correspondingPiece != null)
            capturedPieces.Add(correspondingPiece);
    }

    // GUI
    protected virtual void Awake(){}

    // Start is called before the first frame update
    protected virtual void Start(){}

    // Update is called once per frame
    protected virtual void Update() {   }
}

