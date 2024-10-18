using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BishopBoard : PieceBoard
{
    public BishopBoard(bool isWhite) : base(isWhite)
    {
        Type = 'B';
    }
    public BishopBoard(BishopBoard original) : base(original){

    }
    public override PieceBoard Clone() => new BishopBoard(this);
}
