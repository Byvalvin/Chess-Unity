using System.Collections.Generic;
using UnityEngine;

public class Bot : Player
{

    // Add methods for player actions, like making a move, if needed
}


public abstract class BotState : PlayerState
{
    public GameState CurrentGame{get; set;}
    // Transposition table
    protected Dictionary<string, int> TT = new Dictionary<string, int>();

    public BotState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public BotState(BotState original) : base(original){
        this.CurrentGame = original.CurrentGame;
    }
    public abstract override PlayerState Clone();

    public virtual Vector2Int GetMove(){
        

        Dictionary<int, ulong> moveMap = new Dictionary<int, ulong>();
        foreach (PieceBoard pieceBoard in PieceBoards.Values){
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong validMoves = CurrentGame.GetMovesAllowed(pieceBoard, kvp.Key);
                if(validMoves!=0) moveMap[kvp.Key] = validMoves;
            }
            
        }

        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        Vector2Int completeMove = Evaluate(moveMap);
    
        return completeMove;
    }
    protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone)=>1;// placeholder assumes all moves are equal but diff bots will have diff scoring
    
    protected virtual Vector2Int Evaluate(Dictionary<int, ulong> moveMap){
        return new Vector2Int(8,16);
    }
}
