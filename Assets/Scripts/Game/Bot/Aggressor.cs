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
    public AggressorState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public AggressorState(BotState botState) : base(botState){}
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to) //Prioritize capturing pieces or making aggressive moves
    {
        int score = 1;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;
        
        if (targetPiece != null){
            // If capturing, add the value of the captured piece
            score += pieceValue[targetPiece.Type]+10;
        }else{
            // find a move that increases the number of valid moves a piece the most(to increase the chance to capture)
            // Simulate the move
            GameState clone = currentGame.Clone();
            clone.MakeBotMove(from, to);
            foreach (PieceState pieceState in clone.PlayerStates[TurnIndex].PieceStates)
            {
                score += pieceState.ValidMoves.Count;
            }
        }
        return score; // Return the total score for the move
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
