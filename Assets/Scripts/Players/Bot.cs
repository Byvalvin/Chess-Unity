using System.Collections.Generic;
using UnityEngine;

public class Bot : Player
{

    // Add methods for player actions, like making a move, if needed
}


public abstract class BotState : PlayerState
{
    public GameState CurrentGame{get; protected set;}
    // Transposition table
    protected Dictionary<string, int> TT = new Dictionary<string, int>();

    public BotState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public BotState(BotState original) : base(original){
        this.CurrentGame = original.CurrentGame;
    }
    public abstract override PlayerState Clone();

    public virtual Vector2Int GetMove(){
        return new Vector2Int(8,16);
    }
    protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone)=>1;// placeholder assumes all moves are equal but diff bots will have diff scoring
    

}
