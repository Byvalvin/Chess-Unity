using System.Collections.Generic;

public abstract class PieceBoard
{
    public ulong Bitboard { get; set; }
    public char Type { get; set; }
    public bool IsWhite { get; set; }
    protected Dictionary<int, bool> FirstMoveMap { get; set; }

    public PieceBoard(bool isWhite, ulong startingBitboard = 0)
    {
        IsWhite = isWhite;
        Bitboard = startingBitboard;
        FirstMoveMap = new Dictionary<int, bool>();

        if (startingBitboard != 0)
        {
            for (int i = 0; i < 64; i++)
            {
                if ((startingBitboard & (BitOps.a1 << i)) != 0)
                {
                    FirstMoveMap[i] = true;
                }
            }
        }
    }

    public PieceBoard(PieceBoard original)
    {
        IsWhite = original.IsWhite;
        Bitboard = original.Bitboard;
        FirstMoveMap = new Dictionary<int, bool>(original.FirstMoveMap);
    }

    public abstract PieceBoard Clone();
    public abstract bool CanMove(int index);
    public abstract HashSet<int> ValidMoves(int index);

    public void Move(int fromIndex, int toIndex)
    {
        Bitboard &= ~(BitOps.a1 << fromIndex);
        Bitboard |= (BitOps.a1 << toIndex);

        if (FirstMoveMap.ContainsKey(fromIndex))
        {
            FirstMoveMap[fromIndex] = false;
        }
    }
}
