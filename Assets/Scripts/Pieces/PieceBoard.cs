public abstract class PieceBoard
{
    public ulong Bitboard { get; set; } // Bitboard for this piece type
    public char Type { get; set; } // Type of piece represented by a char (K, Q, R, B, N, P)
    public bool IsWhite { get; set; } // True if the piece is white, false if black

    public PieceBoard(bool isWhite)
    {
        IsWhite = isWhite;
        Bitboard = 0; // Initialize with no pieces
    }

    public PieceBoard (PieceBoard original){
        IsWhite = original.IsWhite;
        Bitboard = original.Bitboard;
    }

    public abstract PieceBoard Clone();
}
