using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnBoard : PieceBoard
{
    public PawnBoard(bool isWhite) : base(isWhite)
    {
        Type = 'P';
    }
    public PawnBoard(PawnBoard original) : base(original){

    }
    public override PieceBoard Clone() => new PawnBoard(this);
}
