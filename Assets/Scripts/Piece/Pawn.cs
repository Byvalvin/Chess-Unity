using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private bool firstMove = true;

    public override bool CanMove(Vector2 to){
        
        if(!InBounds(to*tileSize))
            return false;
            
        bool sameX = currentPos.x==to.x,
            lightAndDown = colour && currentPos.y-1==to.y, //light moves down
            darkAndUp = !colour && currentPos.y+1==to.y;   //dark moves up

        // Normal move for one space
        if ((lightAndDown || darkAndUp) && sameX)
        {
            return true;
        }
        
        // First move double jump
        bool DlightAndDown = colour && currentPos.y-2==to.y, //light moves down
            DdarkAndUp = !colour && currentPos.y+2==to.y;   //dark moves up
        if (firstMove 
            && ((DlightAndDown || DdarkAndUp) && sameX)
            )
        {
            return true;
        }

        // Captures diag
        bool leftOrRightX = Mathf.Abs(currentPos.x - to.x) == 1;
        if((lightAndDown || darkAndUp) && leftOrRightX)
        {
            return true;
        }

        return false;
    }
    public override void Move(Vector2 to)
    {
        if (CanMove(to))
        {
            currentPos = to;
            firstMove = false; // Mark as moved
            SetPosition();
        }
    }

    public override List<Vector2> GetValidMoves(){
        Debug.Log("My current pos"+ currentPos);
        List<Vector2> validMoves = new List<Vector2>();
        
        // one space fwd
        Vector2 fwd = new Vector2(currentPos.x, colour?currentPos.y-1:currentPos.y+1);
        if(CanMove(fwd))
        {
            validMoves.Add(fwd);
        }

        // two spaces fwd, dfwd
        Vector2 dfwd = new Vector2(currentPos.x, colour?currentPos.y-2:currentPos.y+2);
        if(CanMove(dfwd))
        {
            validMoves.Add(dfwd);
        }

        // captures diagonally
        Vector2 diag1 = new Vector2(currentPos.x-1, colour? currentPos.y-1 : currentPos.y+1),
                diag2 = new Vector2(currentPos.x+1, colour? currentPos.y-1 : currentPos.y+1);

        if(CanMove(diag1))
        {
            validMoves.Add(diag1);
        }
        if(CanMove(diag2))
        {
            validMoves.Add(diag2);
        }

        return validMoves;

    }


    // GUI
    public override void HandleInput()
{
    // if (Input.GetMouseButtonDown(0))
    // {
    //     Vector2 targetPosition = GetMouseWorldPosition();
    //     List<Vector2> validMoves = GetValidMoves();

    //     // Check if the target position is in the list of valid moves
    //     if (validMoves.Contains(targetPosition))
    //     {
    //         Move(targetPosition);
    //     }
    // }
}


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        type = "Pawn";

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
