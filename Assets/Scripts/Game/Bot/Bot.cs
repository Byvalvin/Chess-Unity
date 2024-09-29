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
        
        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        Vector2Int[] completeMove = Evaluate(moveMap);
        Vector2Int moveFrom=completeMove[0], moveTo=completeMove[1];
        return new Vector2Int[]{moveFrom, moveTo};
    }
    protected virtual Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        Vector2Int bestFrom = default;
        Vector2Int bestTo = default;
        int bestScore = int.MaxValue;

        foreach (var kvp in moveMap)
        {
            Vector2Int from = kvp.Key;
            foreach (var to in kvp.Value)
            {
                int score = EvaluateMove(from, to);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestFrom = from;
                    bestTo = to;
                }
            }
        }

        return new Vector2Int[] { bestFrom, bestTo };
    }
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