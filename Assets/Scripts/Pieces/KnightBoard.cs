using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightBoard : PieceBoard
{
    public KnightBoard(bool isWhite) : base(isWhite)
    {
        Type = 'N';
    }
    public KnightBoard(KnightBoard original) : base(original){

    }
    public override PieceBoard Clone() => new KnightBoard(this);
}
