using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{

    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        /* 
        in lines y = x, m=1 and y = -x, m=-1 
        but need to determine b based on piece position for both lines
        use y = mx + b;
        */
        int y=(int)Position.y, x=(int)Position.x;

        // y = -1x + b
        int b1 = y + x;
        bool onLine1 = y == -1*x + b1;

        // y = 1x + b
        int b2 = y - x;
        bool onLine2 = y == 1*x + b2;

        return onLine1 || onLine2;
    }
    public override List<Vector2Int> GetValidMoves(){
        List<Vector2Int> validMoves = new List<Vector2Int>();
        Debug.Log("My current pos"+ currentPos);

        int x=Position.x, y=Position.y;

        // topL
        int tLx=x-1, tLy=y+1;
        while(InBounds(new Vector2Int(tLx,tLy)))
        {
            validMoves.Add(new Vector2Int(tLx--, tLy++));
        }

        // botL
        int bLx=x-1, bLy=y-1;
        while(InBounds(new Vector2Int(bLx,bLy)))
        {
            validMoves.Add(new Vector2Int(bLx--, bLy--));
        }

        // botR
        int bRx=x+1, bRy=y-1;
        while(InBounds(new Vector2Int(bRx,bRy)))
        {
            validMoves.Add(new Vector2Int(bRx++, bRy--));
        }

        // topR
        int tRx=x+1, tRy=y+1;
        while(InBounds(new Vector2Int(tRx,tRy)))
        {
            validMoves.Add(new Vector2Int(tRx++, tRy++));
        }


        return validMoves.FindAll(CanMove);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        type = "Bishop";

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
