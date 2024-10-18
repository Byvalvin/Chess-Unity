public class PieceBoard
{
    public ulong Bitboard { get; set; } // Bitboard for this piece type
    public char Type { get; set; } // Type of piece represented by a char (K, Q, R, B, N, P)
    public bool IsWhite { get; set; } // True if the piece is white, false if black

    public PieceBoard(char type, bool isWhite)
    {
        Type = type;
        IsWhite = isWhite;
        Bitboard = 0; // Initialize with no pieces
    }
}
