using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bot : Player
{
    private Game currentGame;

    public override Game CurrentGame{
        get=>currentGame;
        set=>currentGame=value;
    }
    public override Vector2Int[] GetMove()
    {
        //Vector2Int moveFrom = new Vector2Int(3,1), moveTo = new Vector2Int(3,3);
        
        Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        foreach (Piece piece in Pieces)
        {
            HashSet<Vector2Int> validMoves = CurrentGame.GetMovesAllowed(piece);
            if(validMoves.Count!=0) moveMap[piece.Position] = validMoves;
        }

        Debug.Log("MoveMAP");
        foreach (var kvp in moveMap)
        {
            Debug.Log(kvp.Key);
            foreach (var item in kvp.Value)
            {
              Debug.Log(kvp.Key + ":" + item);  
            }
        }
        
        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        
        Vector2Int[] completeMove = Evaluate(moveMap);
        Vector2Int moveFrom=completeMove[0], moveTo=completeMove[1];
        return new Vector2Int[]{moveFrom, moveTo};
    }
    protected abstract Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap);
    protected virtual int EvaluateMove(Vector2Int from, Vector2Int to)=>1; // placeholder assumes all moves are equal but diff bots will have diff scoring
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
