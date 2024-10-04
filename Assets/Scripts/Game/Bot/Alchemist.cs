using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Alchemist: Focuses on piece exchanges, valuing trades that lead to a favorable material advantage.
Creates synergies between pieces.  consider capturing pieces, controlling the center, etc.
*/

public class AlchemistState : BotState
{
    public AlchemistState(string _playerName, bool _colour) : base(_playerName, _colour){}
    public AlchemistState(BotState botState) : base(botState){}

    protected override int EvaluateMove(Vector2Int from, Vector2Int to)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1a. Capture Bonus
        if (targetPiece != null){
            // If capturing, add the value of the captured piece
            score += pieceValue[targetPiece.Type]+5;

            // but is it defended?
            int nDefenders = PieceDefended(currentGame, targetPiece, to);
            score += (-5*nDefenders);
            // If the piece is highly defended, reduce the score significantly
        }

        // 1. Evaluate potential piece exchange
        score += EvaluatePieceExchange(clone, movingPiece, targetPiece);

        // 2. Central Control
        score += 2*CentralControlBonus(to, clone);

        // 3. Synergies
        score += EvaluatePieceSynergies(clone, to);

        // 4. Piece Safety
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 6. Army value impact
        //score += ArmyValue(clone, true) - ArmyValue(clone, false);

        // 7. Check for threats to the king
        score += AttackedKingTiles(clone);

        Debug.Log(movingPiece.Type+movingPiece.Colour + from + to + score);

        return score;
    }

    private int EvaluatePieceSynergies(GameState gameState,  Vector2Int to)
    {
        int synergyScore = 0;

        // Reward positioning that allows pieces to protect each other
        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            if (piece.Position != to)
            {
                // Check for proximity
                if (piece.ValidMoves.Contains(to))
                {
                    synergyScore += 1; // Reward close support
                }

                // Reward formations that can provide additional support
                if (IsFormationStrong(gameState, to))
                {
                    synergyScore += 1; // Bonus for strong formations
                }
            }
        }

        return synergyScore;
    }

    private bool IsFormationStrong(GameState gameState, Vector2Int moveTo)
    {
        // Get the current player's pieces
        var myPieces = gameState.PlayerStates[TurnIndex].PieceStates;

        // Check for knight formations
        if (gameState.GetTile(moveTo).pieceState.Type == "Knight" && IsKnightSupported(moveTo, myPieces))
        {
            return true;
        }

        // Check for bishop formations
        if (gameState.GetTile(moveTo).pieceState.Type == "Bishop" && AreBishopsAligned(moveTo, myPieces))
        {
            return true;
        }

        // Check for pawn formations
        if (gameState.GetTile(moveTo).pieceState.Type == "Pawn" && ArePawnsSupported(moveTo, myPieces, gameState.GetTile(moveTo).pieceState.Colour))
        {
            return true;
        }

        return false; // Default to not strong if no conditions met
    }

    // Check if a knight is supported by another knight
    private bool IsKnightSupported(Vector2Int knightPos, List<PieceState> myPieces)
    {
        return false;
    }

    // Check if bishops are aligned
    private bool AreBishopsAligned(Vector2Int bishopPos, List<PieceState> myPieces)
    {
        return false;
    }

    // Check if pawns are supporting each other
    private bool ArePawnsSupported(Vector2Int pawnPos, List<PieceState> myPieces, bool colour)
    {
        Vector2Int leftSupport = new Vector2Int(pawnPos.x - 1, pawnPos.y + (colour?-1:1));
        Vector2Int rightSupport = new Vector2Int(pawnPos.x + 1, pawnPos.y + (colour?-1:1));

        // Check if adjacent pawns exist
        foreach (var piece in myPieces)
        {
            if (piece.Type == "Pawn")
            {
                if (piece.Position == leftSupport || piece.Position == rightSupport)
                {
                    return true; // Pawns are supporting each other
                }
            }
        }
        return false;
    }

    // Helper method to check if two positions are on the same diagonal
    private bool IsOnSameDiagonal(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) == Mathf.Abs(pos1.y - pos2.y);
    }


    private int EvaluatePieceExchange(GameState gameState, PieceState movingPiece, PieceState targetPiece)
    {
        if (targetPiece != null && PieceDefended(gameState, targetPiece, targetPiece.Position)<=2) // Capturing
        {
            return 10*(pieceValue[targetPiece.Type] - pieceValue[movingPiece.Type]);
        }
        return 0; // No exchange
    }
}
public class Alchemist : Bot
{
    protected override void Awake()
    {
        //state = new AlchemistState();
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
