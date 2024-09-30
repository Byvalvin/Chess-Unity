using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerState
{
    private string playerName;
    private bool colour = true; //assume white
    private int turnIndex = 0; //assume white turn
    private float tileSize;
    private List<Piece> pieces = new List<Piece>(), captured = new List<Piece>();

    private bool inCheck = true, doubleCheck = true;

    public Piece KingAttacker = null; // the opposing piece attacking player's king 

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

    public PlayerState(){}
    public PlayerState(PlayerState original){
        this.playerName = original.playerName;
        this.colour = original.colour; //assume white
        this.turnIndex = original.turnIndex; //assume white turn
        this.tileSize = original.tileSize;
        this.pieces = new List<Piece>(original.pieces);
        this.captured = new List<Piece>(original.captured);

        this.inCheck = original.inCheck;
        this.doubleCheck = original.doubleCheck;

        this.KingAttacker = original.KingAttacker; // the opposing piece attacking player's king 
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

    public virtual Vector2Int[] GetMove()
    {
        Vector2Int[] fromTo = new Vector2Int[2];
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / state.TileSize);
        fromTo[1] = targetPosition;
        return fromTo;
    }
}

public class Player : MonoBehaviour
{
   protected PlayerState state;

   public PlayerState State{
       get=>state;
       set=>state=value;
   }

    // GUI
    protected virtual void Awake()
    {
     state = new PlayerState();   
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}

