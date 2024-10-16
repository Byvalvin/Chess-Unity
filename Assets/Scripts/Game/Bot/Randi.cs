using System.Collections.Generic;
using UnityEngine;

// Randi: Makes random moves, adding an element of unpredictability.
public class RandiState : BotState
{
    public RandiState(string playerName, bool color) : base(playerName, color) { }
    public RandiState(RandiState original) : base(original) { }
    public override PlayerState Clone() => new RandiState(this);

    protected override Vector2Int[] Evaluate(Dictionary<Vector2Int, HashSet<Vector2Int>> moveMap)
    {
        if (moveMap.Count == 0){
            Debug.Log("Randi has no more moves left onfortunately");
            return null; // No moves available
        }

        // Get a random key from the dictionary
        int randomKeyIndex = Random.Range(0, moveMap.Count);
        Vector2Int randomFrom = new List<Vector2Int>(moveMap.Keys)[randomKeyIndex];

        // Get valid moves for the selected key
        var validMoves = moveMap[randomFrom];
        if (validMoves.Count == 0) return null; // No valid moves available for this key

        // Select a random move from the valid moves
        int randomMoveIndex = Random.Range(0, validMoves.Count);
        Vector2Int randomTo = new List<Vector2Int>(validMoves)[randomMoveIndex];

        if(GameState.IsPromotion(currentGame.GetTile(randomFrom).pieceState,randomTo))
            PromoteTo = new string[]{ "Queen", "Rook", "Bishop", "Knight" }[Random.Range(0, 4)];
           

        // Return the random move
        return new Vector2Int[] { randomFrom, randomTo };
    }
}


public class Randi : Bot
{
    protected override void Awake()
    {
        //state = new RandiState();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}


