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
    static int aggressiveBoost = 5;
    public AggressorState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public AggressorState(BotState botState) : base(botState){}
    public override PlayerState Clone() => new AggressorState(this); 
    
    protected override int EvaluateMove(Vector2Int from, Vector2Int to) //Prioritize capturing pieces or making aggressive moves
    {
       
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);
                
         // 1. Capture Bonus
        if (targetPiece != null)
            // If capturing, add the value of the captured piece
            score += pieceValue[targetPiece.Type]+aggressiveBoost;
        
        // 2. Central Control
        score += CentralControlBonus(to, clone);

        // 3. Mobility
        // find a move that increases the number of valid moves a piece the most(to increase the chance to capture)
        foreach (PieceState pieceState in clone.PlayerStates[TurnIndex].PieceStates)
        {
            score += pieceState.ValidMoves.Count;
        }

        // 4. Piece Saftety
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 5. Check the value of my pieces
        //score += ArmyValue(clone, true);

        // last. Adjust Aggressiveness
        score = AdjustAggressiveness(score);

        
        Debug.Log(movingPiece.Type+movingPiece.Colour + from + to + score);
        return score; // Return the total score for the move
    }

    private int ArmyValue(GameState gameState,bool myArmy=true){
        int av = 0;
        foreach (PieceState piece in gameState.PlayerStates[myArmy ? TurnIndex : 1-TurnIndex].PieceStates)
            if(piece is not KingState)
                av+=pieceValue[piece.Type];
        return av;
    }

    private int CentralControlBonus(Vector2Int position, GameState gameState)
    {
        // Implement a method to calculate score based on board control
        // Example: add 1 point for controlling the center squares
        int centreControl = InCenter(position)? 2:0;
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates){
            centreControl += Utility.FindAll(piece.validMoves, InCenter).Count;
        }
            
        return centreControl;
    }

    private int EvaluatePieceSafety(Vector2Int from, Vector2Int to, string type, GameState gameState)
    {

        int toNotSafe = 0, fromNotSafe = 0; // must move pieces under attack, dont go to places under attack
        foreach (PieceState opponentPiece in gameState.PlayerStates[1-TurnIndex].PieceStates)
            if(opponentPiece.ValidMoves.Contains(to))
                toNotSafe-=5; // dont go there
        foreach (PieceState opponentPiece in currentGame.PlayerStates[1-TurnIndex].PieceStates)
            if(opponentPiece.ValidMoves.Contains(from))
                fromNotSafe+=10; // dont stay here
        

        // Factor in piece value for safety evaluation
        return (toNotSafe + fromNotSafe)*pieceValue[type]; // Higher penalty for more valuable pieces
    }

    private int AdjustAggressiveness(int score)
    {
        int myArmyValue = ArmyValue(currentGame, true);
        int opponentArmyValue = ArmyValue(currentGame, false);

        if (myArmyValue > opponentArmyValue)
            score += 10; // More aggressive if ahead
        else if (myArmyValue < opponentArmyValue)
            score -= 10; // More cautious if behind

        return score;
    }


    PieceState FutureState(Vector2Int to, GameState gameState, bool isSelf=true){
        foreach (PieceState piece in gameState.PlayerStates[isSelf? TurnIndex:1-TurnIndex].PieceStates){
            if(piece.Position==to)
                return piece;
        }
        return null;
    }

    bool InCenter(Vector2Int position){
        HashSet<int> centerCoords = new HashSet<int>{3,4};
        return centerCoords.Contains(position.x) && centerCoords.Contains(position.y);
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
