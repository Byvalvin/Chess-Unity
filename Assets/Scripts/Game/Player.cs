using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string playerName;
    private bool colour = true; //assume white
    private List<Piece> pieces = new List<Piece>();

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
        set => pieces = value;
    }

    public void AddPiece(Piece piece)
    {
        pieces.Add(piece);
    }

    public void RemovePiece(Piece piece)
    {
        pieces.Remove(piece);
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
