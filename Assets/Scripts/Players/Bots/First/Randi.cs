
public class Randi : Player
{

    // Add methods for player actions, like making a move, if needed
}


public class RandiState : BotState
{

    public RandiState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public RandiState(RandiState original) : base(original){
    }
    public override PlayerState Clone() => new RandiState(this);


    //protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone)=>1;// placeholder assumes all moves are equal but diff Randis will have diff scoring
    

}
