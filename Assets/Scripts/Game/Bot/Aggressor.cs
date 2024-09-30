using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Aggressor: Always looking to capture enemy pieces, favoring aggressive plays.
 Strong offensive moves, aiming for maximum damage.
 Knight: Uses direct and aggressive tactics to pressure opponents, favoring offensive play
*/
public class Aggressor : Bot
{
    protected override int EvaluateMove(Vector2Int from, Vector2Int to) //Prioritize capturing pieces or making aggressive moves
    {
        int score = 1;
        Piece movingPiece = CurrentGame.GetTile(from).piece;
        Piece targetPiece = CurrentGame.GetTile(to).piece;
        
        if (targetPiece != null){
            // If capturing, add the value of the captured piece
            score += pieceValue[targetPiece.Type]+10;
        }else{
            // find a move that increases the number of valid moves a piece the most(to increase the chance to capture)
            // Simulate the move
        }


        return score; // Return the total score for the move
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
