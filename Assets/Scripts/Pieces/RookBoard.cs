using System.Collections.Generic;

public class RookBoard : PieceBoard
{
    public RookBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'R';
    }

    public RookBoard(RookBoard original) : base(original) { }

    public override PieceBoard Clone() => new RookBoard(this);
    public override HashSet<int> ValidMoves(ulong fullBoard, int index)
    {
        HashSet<int> validMoves = new HashSet<int>();

        // Horizontal and vertical movements
        for (int i = 1; i < 8; i++)
        {
            // Up
            int upIndex = BitOps.ForwardMove(index, i);
            if (!BitOps.InBounds(upIndex)) break;
            validMoves.Add(upIndex);
            if ((Bitboard & (BitOps.a1 << upIndex)) != 0) break; // Stop if there's a piece

            // Down
            int downIndex = BitOps.BackwardMove(index, i);
            if (!BitOps.InBounds(downIndex)) break;
            validMoves.Add(downIndex);
            if ((Bitboard & (BitOps.a1 << downIndex)) != 0) break; // Stop if there's a piece

            // Left
            int leftIndex = BitOps.LeftMove(index, i);
            if (!BitOps.InBounds(leftIndex)) break;
            validMoves.Add(leftIndex);
            if ((Bitboard & (BitOps.a1 << leftIndex)) != 0) break; // Stop if there's a piece

            // Right
            int rightIndex = BitOps.RightMove(index, i);
            if (!BitOps.InBounds(rightIndex)) break;
            validMoves.Add(rightIndex);
            if ((Bitboard & (BitOps.a1 << rightIndex)) != 0) break; // Stop if there's a piece
        }

        return validMoves;
    }
}
