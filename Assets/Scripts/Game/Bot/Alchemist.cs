using System.Collections.Generic;
using UnityEngine;

/*
Alchemist: Focuses on piece exchanges, valuing trades that lead to a favorable material advantage.
Creates synergies between pieces, considering capturing, controlling the center, etc.
*/

public class AlchemistState : BotState
{
    private const int CaptureBonus = 5;
    private const int ExchangeValueMultiplier = 10;
    private const int CentralControlMultiplier = 2;

    public AlchemistState(string playerName, bool colour) : base(playerName, colour) { }
    public AlchemistState(AlchemistState original) : base(original) { }
    public override PlayerState Clone() => new AlchemistState(this);

    protected override int EvaluateMove(Vector2Int from, Vector2Int to, GameState clone)
    {
        int score = 0;
        PieceState movingPiece = CurrentGame.GetTile(from).pieceState;
        PieceState targetPiece = CurrentGame.GetTile(to).pieceState;

        // Simulate the move
        //GameState clone = currentGame.Clone();
        clone.MakeBotMove(from, to);

        // 1. Capture Bonus
        score += EvaluateCaptureBonus(targetPiece);

        // 2. Evaluate potential piece exchange
        score += EvaluatePieceExchange(clone, movingPiece, targetPiece);

        // 3. Central Control
        score += CentralControlMultiplier * CentralControlBonus(to, clone);

        // 4. Synergies
        score += EvaluatePieceSynergies(clone, to);

        // 5. Piece Safety
        score += EvaluatePieceSafety(from, to, movingPiece.Type, clone);

        // 6. Check for threats to the king
        score += AttackedKingTiles(clone);

        Debug.Log($"{movingPiece.Type} {movingPiece.Colour} {from} {to} {score}");

        return score;
    }

    private int EvaluateCaptureBonus(PieceState targetPiece)
    {
        if (targetPiece == null) return 0;

        int score = pieceValue[targetPiece.Type] + CaptureBonus;
        int nDefenders = PieceDefended(currentGame, targetPiece, targetPiece.Position);
        score += -5 * nDefenders; // Penalize for highly defended pieces

        return score;
    }

    private int EvaluatePieceSynergies(GameState gameState, Vector2Int to)
    {
        int synergyScore = 0;

        foreach (PieceState piece in gameState.PlayerStates[TurnIndex].PieceStates)
        {
            if (piece.Position != to)
            {
                synergyScore += piece.ValidMoves.Contains(to) ? 1 : 0; // Reward close support
                synergyScore += IsFormationStrong(gameState, to) ? 1 : 0; // Bonus for strong formations
            }
        }

        return synergyScore;
    }

    private bool IsFormationStrong(GameState gameState, Vector2Int moveTo)
    {
        string pieceType = gameState.GetTile(moveTo).pieceState.Type;

        return pieceType switch
        {
            "Knight" => IsKnightSupported(moveTo, gameState.PlayerStates[TurnIndex].PieceStates),
            "Bishop" => AreBishopsAligned(moveTo, gameState.PlayerStates[TurnIndex].PieceStates),
            "Pawn" => ArePawnsSupported(moveTo, gameState.PlayerStates[TurnIndex].PieceStates, gameState.GetTile(moveTo).pieceState.Colour),
            _ => false
        };
    }

    private bool IsKnightSupported(Vector2Int knightPos, List<PieceState> myPieces)
    {
        // Implement logic to check knight support
        return false;
    }

    private bool AreBishopsAligned(Vector2Int bishopPos, List<PieceState> myPieces)
    {
        // Implement logic to check bishop alignment
        return false;
    }

    private bool ArePawnsSupported(Vector2Int pawnPos, List<PieceState> myPieces, bool colour)
    {
        Vector2Int leftSupport = new Vector2Int(pawnPos.x - 1, pawnPos.y + (colour ? -1 : 1));
        Vector2Int rightSupport = new Vector2Int(pawnPos.x + 1, pawnPos.y + (colour ? -1 : 1));

        foreach (var piece in myPieces)
        {
            if (piece.Type == "Pawn" && (piece.Position == leftSupport || piece.Position == rightSupport))
            {
                return true; // Pawns are supporting each other
            }
        }
        return false;
    }

    private int EvaluatePieceExchange(GameState gameState, PieceState movingPiece, PieceState targetPiece)
    {
        if (targetPiece != null && PieceDefended(gameState, targetPiece, targetPiece.Position) <= 2) // Capturing
        {
            return ExchangeValueMultiplier * (pieceValue[targetPiece.Type] - pieceValue[movingPiece.Type]);
        }
        return 0; // No exchange
    }
}

public class Alchemist : Bot
{
    protected override void Awake()
    {
        // Initialize AlchemistState if needed
        // state = new AlchemistState();
    }

    protected override void Start() { }

    protected override void Update() { }
}
