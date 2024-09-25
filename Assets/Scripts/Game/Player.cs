using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string playerName;
    private bool colour = true; //assume white
    private List<Piece> pieces = new List<Piece>(), captured = new List<Piece>();

    private bool inCheck = false, doubleCheck = false;

    public Piece KingAttacker = null; // the opposing piece attacking player's king 

    public string PlayerName{
        get{return playerName;}
        set{playerName=value;}
    }
    public bool Colour{
        get{return colour;}
        set{colour=value;}
    }
    public List<Piece> Pieces
    {
        get => pieces;
    }

    public bool InCheck{
        get{return inCheck;}
        set{
            inCheck=value;
        }
    }
    public bool DoubleCheck{
        get{return doubleCheck;}
        set{
            doubleCheck=value;
        }
    }
    public bool IsInCheck(){
        return doubleCheck || InCheck;
    }

    public void AddPiece(Piece piece)
    {
        pieces.Add(piece);
    }

    public void RemovePiece(Piece piece)
    {
        pieces.Remove(piece);
    }

    public void Capture(Piece piece)
    {
        captured.Add(piece);
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
