using System.Collections.Generic;

public class PlayerState
{
    public string PlayerType { get; set; } // Type of player
    public bool IsWhite { get; set; } // True for white, false for black
    public Dictionary<char, PieceBoard> PieceBoards { get; set; } // Dictionary of piece boards

    public PlayerState(string playerType, bool isWhite)
    {
        PlayerType = playerType;
        IsWhite = isWhite;
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
        // Example initialization for pieces' starting positions
        if (IsWhite)
        {
            PieceBoards['K'].Bitboard = 0x0000000000000004; // King on e1
            PieceBoards['Q'].Bitboard = 0x0000000000000008; // Queen on d1
            PieceBoards['B'].Bitboard = 0x0000000000000024; // Bishops on c1 and f1
            PieceBoards['N'].Bitboard = 0x0000000000000042; // Knights on b1 and g1
            PieceBoards['R'].Bitboard = 0x0000000000000081; // Rooks on a1 and h1
            PieceBoards['P'].Bitboard = 0x000000000000FF00; // Pawns on 2nd rank
        }
        else
        {
            PieceBoards['K'].Bitboard = 0x0400000000000000; // King on e8
            PieceBoards['Q'].Bitboard = 0x0800000000000000; // Queen on d8
            PieceBoards['B'].Bitboard = 0x2400000000000000; // Bishops on c8 and f8
            PieceBoards['N'].Bitboard = 0x4200000000000000; // Knights on b8 and g8
            PieceBoards['R'].Bitboard = 0x8100000000000000; // Rooks on a8 and h8
            PieceBoards['P'].Bitboard = 0x00FF000000000000; // Pawns on 7th rank
        }
    }
}
