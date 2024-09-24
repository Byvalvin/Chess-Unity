using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{

    public override bool CanMove(Vector2Int to){
            return true;
    }
    protected override void SetValidMoves(){
        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();
        validMoves = moves;
    }

    protected override void Awake(){
        type = "Queen";
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetPosition();
        SetSprite();   
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
    }
}
