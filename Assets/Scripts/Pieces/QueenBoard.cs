using System.Collections.Generic;

public class QueenBoard : PieceBoard
{
    public QueenBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'Q';
    }

    public QueenBoard(QueenBoard original) : base(original) { }

    public override PieceBoard Clone() => new QueenBoard(this);
    public override HashSet<int> ValidMoves(ulong fullBoard, int index)
    {
        HashSet<int> validMoves = new HashSet<int>();

        // Combine rook and bishop moves
        validMoves.UnionWith(new RookBoard(IsWhite, Bitboard).ValidMoves(fullBoard, index));
        validMoves.UnionWith(new BishopBoard(IsWhite, Bitboard).ValidMoves(fullBoard, index));

        return validMoves;
    }
}
