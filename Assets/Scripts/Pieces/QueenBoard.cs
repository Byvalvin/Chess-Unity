using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBoard : PieceBoard
{
    public QueenBoard(bool isWhite) : base(isWhite)
    {
        Type = 'Q';
    }
    public QueenBoard(QueenBoard original) : base(original){

    }
    public override PieceBoard Clone() => new QueenBoard(this);
}
