using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
        public override bool CanMove(Vector2 to){
            return true;
    }
    public override void Move(Vector2 to){}
    public override List<Vector2> GetValidMoves(){
        List<Vector2> validMoves = new List<Vector2>();
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
