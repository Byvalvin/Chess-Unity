using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override bool CanMove(Vector2Int to){
            return true;
    }
    public override List<Vector2Int> GetValidMoves(){
        List<Vector2Int> validMoves = new List<Vector2Int>();
        return validMoves;
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
