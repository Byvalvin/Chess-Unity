using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingBoard : PieceBoard
{
    public KingBoard(bool isWhite) : base(isWhite)
    {
        Type = 'K';
    }
    public KingBoard(KingBoard original) : base(original){

    }
    public override PieceBoard Clone() => new KingBoard(this);
}
