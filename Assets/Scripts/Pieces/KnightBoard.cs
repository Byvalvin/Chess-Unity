using System.Collections.Generic;

public class KnightBoard : PieceBoard
{
    public KnightBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'N';
    }

    public KnightBoard(KnightBoard original) : base(original) { }

    public override PieceBoard Clone() => new KnightBoard(this);

    public override bool CanMove(int index)
    {
        return ValidMoves(index).Count > 0;
    }

    public override HashSet<int> ValidMoves(int index)
    {
        HashSet<int> validMoves = new HashSet<int>();

        int[] knightMoves = new int[]
        {
            -17, -15, -10, -6, 6, 10, 15, 17 // All knight move offsets
        };

        foreach (var move in knightMoves)
        {
            int targetIndex = index + move;
            if (BitOps.InBounds(targetIndex))
            {
                validMoves.Add(targetIndex);
            }
        }

        return validMoves;
    }
}
