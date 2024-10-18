using System.Collections.Generic;

public class PlayerState
{
    public string PlayerType { get; set; } // Type of player
    public bool IsWhite { get; set; } // True for white, false for black
    private bool IsTop { get; set; } // True if white is at the top

    public Dictionary<char, PieceBoard> PieceBoards { get; set; } // Dictionary of piece boards

    public PlayerState(string playerType, bool isWhite)
    {
        PlayerType = playerType;
        IsWhite = isWhite;
        IsTop = isWhite; // Set IsTop based on IsWhite; adjust as needed

        PieceBoards = new Dictionary<char, PieceBoard>
        {
            { 'K', new PieceBoard('K', isWhite) }, // King
            { 'Q', new PieceBoard('Q', isWhite) }, // Queen
            { 'B', new PieceBoard('B', isWhite) }, // Bishop
            { 'N', new PieceBoard('N', isWhite) }, // Knight
            { 'R', new PieceBoard('R', isWhite) }, // Rook
            { 'P', new PieceBoard('P', isWhite) }  // Pawn
        };
    }

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
