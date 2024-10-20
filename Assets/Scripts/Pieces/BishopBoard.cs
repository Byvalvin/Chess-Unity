using System.Collections.Generic;

public class BishopBoard : PieceBoard
{
    public BishopBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'B';
    }

    public BishopBoard(BishopBoard original) : base(original) { }

    public override PieceBoard Clone() => new BishopBoard(this);

    public override HashSet<int> ValidMoves(ulong fullBoard, int index)
    {
        HashSet<int> validMoves = new HashSet<int>();

        // Diagonal movements
        for (int i = 1; i < 8; i++)
        {
            // Up-left
            int upLeftIndex = BitOps.Diagonal1Move(index, i);
            if (!BitOps.InBounds(upLeftIndex)) break;
            validMoves.Add(upLeftIndex);
            if ((Bitboard & (BitOps.a1 << upLeftIndex)) != 0) break;

            // Up-right
            int upRightIndex = BitOps.Diagonal2Move(index, i);
            if (!BitOps.InBounds(upRightIndex)) break;
            validMoves.Add(upRightIndex);
            if ((Bitboard & (BitOps.a1 << upRightIndex)) != 0) break;

            // Down-left
            int downLeftIndex = BitOps.Diagonal3Move(index, i);
            if (!BitOps.InBounds(downLeftIndex)) break;
            validMoves.Add(downLeftIndex);
            if ((Bitboard & (BitOps.a1 << downLeftIndex)) != 0) break;

            // Down-right
            int downRightIndex = BitOps.Diagonal4Move(index, i);
            if (!BitOps.InBounds(downRightIndex)) break;
            validMoves.Add(downRightIndex);
            if ((Bitboard & (BitOps.a1 << downRightIndex)) != 0) break;
        }

        return validMoves;
    }
}
