using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public override bool CanMove(Vector2Int to)
    {
        if (!InBounds(to)) return false;

        // Check if the move is diagonal
        int deltaX = Mathf.Abs(currentPos.x - to.x);
        int deltaY = Mathf.Abs(currentPos.y - to.y);

        return deltaX == deltaY; // Diagonal moves only
    }

    protected override void SetValidMoves()
    {
        //validMoves.Clear();

        HashSet<Vector2Int> moves = new HashSet<Vector2Int>();
        //Debug.Log("My current pos: " + currentPos);

        int x = currentPos.x;
        int y = currentPos.y;

        // Directions: top-left, bottom-left, bottom-right, top-right
        Vector2Int[] directions = {
            new Vector2Int(-1, 1),   // top-left
            new Vector2Int(-1, -1),  // bottom-left
            new Vector2Int(1, -1),   // bottom-right
            new Vector2Int(1, 1)     // top-right
        };

        foreach (var direction in directions)
        {
            int tX = x, tY = y;

            while (true)
            {
                tX += direction.x;
                tY += direction.y;

                if (!InBounds(new Vector2Int(tX, tY))) break;

                moves.Add(new Vector2Int(tX, tY));
            }
        }

        // Filter valid moves using HashSet
        validMoves = FindAll(moves);

    }

    protected override void Awake(){
        base.Awake();
        type = "Bishop";
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
