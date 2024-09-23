using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{


    public override bool CanMove(Vector2Int to){
            return true;
    }
    public override void SetValidMoves(){
        List<Vector2Int> moves = new List<Vector2Int>();
        validMoves = moves;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        type = "Knight";

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
