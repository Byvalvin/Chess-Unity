using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookBoard : PieceBoard
{
    public RookBoard(bool isWhite) : base(isWhite)
    {
        Type = 'R';
    }
    public RookBoard(RookBoard original) : base(original){

    }
    public override PieceBoard Clone() => new RookBoard(this);

}
