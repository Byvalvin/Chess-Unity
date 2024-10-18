using System.Collections.Generic;

public class PlayerState
{
    public string PlayerType { get; set; } // Type of player
    public bool IsWhite { get; set; } // True for white, false for black
    public const bool IsTop = true; // IsTop says who is at bottom; true->white at bottom

    public Dictionary<char, PieceBoard> PieceBoards { get; set; } // Dictionary of piece boards

    public PlayerState(string playerType, bool isWhite)
    {
        PlayerType = playerType;
        IsWhite = isWhite;

        PieceBoards = new Dictionary<char, PieceBoard>
        {
            { 'K', new KingBoard(isWhite) }, // King
            { 'Q', new QueenBoard(isWhite) }, // Queen
            { 'B', new BishopBoard(isWhite) }, // Bishop
            { 'N', new KnightBoard(isWhite) }, // Knight
            { 'R', new RookBoard(isWhite) }, // Rook
            { 'P', new PawnBoard(isWhite) }  // Pawn
        };
    }
    public PlayerState(PlayerState original){
        PlayerType = original.PlayerType;
        IsWhite = original.IsWhite;
        foreach(char type in original.PieceBoards.Keys)
            PieceBoards[type] = original.PieceBoards[type].Clone();
    }
    public PlayerState Clone() => new PlayerState(this);

    public void InitializePieces()
    {
        // Initialize pieces' starting positions using ternary operator
        PieceBoards['K'].Bitboard = IsTop ? (ulong)0x0000000000000010 : (ulong)0x1000000000000000; // King
        PieceBoards['Q'].Bitboard = IsTop ? (ulong)0x0000000000000008 : (ulong)0x0800000000000000; // Queen
        PieceBoards['B'].Bitboard = IsTop ? (ulong)0x0000000000000024 : (ulong)0x2400000000000000; // Bishops
        PieceBoards['N'].Bitboard = IsTop ? (ulong)0x0000000000000042 : (ulong)0x4200000000000000; // Knights
        PieceBoards['R'].Bitboard = IsTop ? (ulong)0x0000000000000081 : (ulong)0x8100000000000000; // Rooks
        PieceBoards['P'].Bitboard = IsTop ? (ulong)0x000000000000FF00 : (ulong)0x00FF000000000000; // Pawns
    }

}
