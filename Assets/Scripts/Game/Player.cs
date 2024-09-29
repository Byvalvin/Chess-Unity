using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    private string playerName;
    private bool colour = true; //assume white
    private int turnIndex = 0; //assume white turn
    private float tileSize;
    private List<Piece> pieces = new List<Piece>(), captured = new List<Piece>();

    private bool inCheck = true, doubleCheck = true;

    public Piece KingAttacker = null; // the opposing piece attacking player's king 


    public Player(Player original){ // Copy constructor
        this.playerName = original.playerName;
        this.colour = original.colour;
        this.turnIndex = original.turnIndex;
        this.tileSize = original.tileSize;
        this.inCheck = original.inCheck;
        this.doubleCheck = original.doubleCheck;

        // Deep copy of pieces and captured lists
        this.pieces = new List<Piece>();
        foreach (var piece in original.pieces)
            this.pieces.Add(piece != null ? (Piece)Activator.CreateInstance(piece.GetType(), piece) : null); // Use the copy constructor

        this.captured = new List<Piece>();
        foreach (var piece in original.captured)
        {
            this.captured.Add(piece != null ? (Piece)Activator.CreateInstance(piece.GetType(), piece) : null); // Use the copy constructor
        }

        // If KingAttacker exists, clone it
        this.KingAttacker = original.KingAttacker != null ? (Piece)Activator.CreateInstance(original.KingAttacker.GetType(), original.KingAttacker) : null; // Clone if present
    }

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
    public float TileSize
    {
        get=>tileSize;
        set=>tileSize=value;
    }
    public List<Piece> Pieces
    {
        get=>pieces;
    }

    public bool InCheck{
        get=>inCheck;
        set=>inCheck=value;
    }
    public bool DoubleCheck{
        get{return doubleCheck;}
        set=>doubleCheck=value;

    }

    public Piece GetKing() => Pieces[0];
    public bool IsInCheck(){
        return doubleCheck || InCheck;
    }

    public void AddPiece(Piece piece) => pieces.Add(piece);
    public void RemovePiece(Piece piece) => pieces.Remove(piece);

    public void Capture(Piece piece) => captured.Add(piece);

    // for only bot inheirtence
    public virtual Game CurrentGame{
        get; set;
    }

    // GUI
    public virtual Vector2Int[] GetMove()
    {
        Vector2Int[] fromTo = new Vector2Int[2];
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / tileSize);
        fromTo[1] = targetPosition;
        return fromTo;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
