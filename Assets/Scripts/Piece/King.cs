using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override bool CanMove(Vector2Int to){
        if (!InBounds(to)) return false;
        return true;
    }
    public override void SetValidMoves(){
        List<Vector2Int> moves = Utility.GetSurroundingPoints(currentPos);
        validMoves =  moves.FindAll(CanMove);
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        type = "King";

        SetPosition();

        // Load sprite
        //pieceSprite = Resources.Load<Sprite>("Pawn");
     
        SetSprite();   
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
    }
}
