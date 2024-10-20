using System.Collections.Generic;

public class KingBoard : PieceBoard
{
    public KingBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'K';
    }

    public KingBoard(KingBoard original) : base(original) { }

    public override PieceBoard Clone() => new KingBoard(this);
    public override HashSet<int> ValidMoves(int index)
    {
        HashSet<int> validMoves = new HashSet<int>();

        int[] kingMoves = new int[]
        {
            -1, 1, -8, 8, -9, -7, 7, 9 // All king move offsets
        };

        foreach (var move in kingMoves)
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
