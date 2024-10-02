using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Aggressor: Always looking to capture enemy pieces, favoring aggressive plays.
 Strong offensive moves, aiming for maximum damage.
 Knight: Uses direct and aggressive tactics to pressure opponents, favoring offensive play
*/

public class AggressorState : BotState
{
    static int aggressiveBoost = 100;
    public AggressorState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public AggressorState(BotState botState) : base(botState){}
    public override PlayerState Clone() => new AggressorState(this); 
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to) //Prioritize capturing pieces or making aggressive moves
    {
       
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;
                
         // 1. Capture Bonus
        if (targetPiece != null)
            // If capturing, add the value of the captured piece
            score += pieceValue[targetPiece.Type]+aggressiveBoost;
        
        // 2. Central Control
        score += CentralControlBonus(to);

        // 3. Mobility
        // find a move that increases the number of valid moves a piece the most(to increase the chance to capture)
        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);
        foreach (PieceState pieceState in clone.PlayerStates[TurnIndex].PieceStates)
        {
            score += pieceState.ValidMoves.Count;
        }

        // 4. Piece Saftety
        score += EvaluatePieceSafety(movingPiece, clone);

        
        //Debug.Log(movingPiece.Type+movingPiece.Colour + from + to + score);
        return score; // Return the total score for the move
    }

    private int CentralControlBonus(Vector2Int position)
    {
        // Implement a method to calculate score based on board control
        // Example: add 1 point for controlling the center squares
        if (position.x == 3 || position.x == 4 || position.y == 3 || position.y == 4)
        {
            return 1; // Adjust as needed
        }
        return 0;
    }

    private int EvaluatePieceSafety(PieceState pieceState, GameState gameState)
    {
        int dangerCount = 0;

        foreach (PieceState opponentPiece in gameState.PlayerStates[1-TurnIndex].PieceStates)
        {
            foreach (Vector2Int move in opponentPiece.ValidMoves)
            {
                if (move == pieceState.Position)
                {
                    dangerCount--;
                }
            }
        }

        // Return a penalty score based on the number of attackers
        return dangerCount * 10; // Adjust the multiplier as needed
    }

}




public class Aggressor : Bot
{
    
    protected override void Awake()
    {
        //state = new AggressorState();
    }
    
    // Start is called before the first frame update
    protected override void Start(){
        
    }

    // Update is called once per frame
    protected override void Update(){
        
    }
}
