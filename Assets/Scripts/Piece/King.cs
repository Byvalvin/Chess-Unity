using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public King(King original) : base(original){}
    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;
        return true;
    }

    protected override void SetValidMoves()
    {
        HashSet<Vector2Int> moves = Utility.GetSurroundingPoints(currentPos);
        validMoves = FindAll(moves);

    }

    protected override void Awake(){
        base.Awake();
        type = "King";
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetSprite();   
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
