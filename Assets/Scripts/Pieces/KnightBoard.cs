using System.Collections.Generic;

public class KnightBoard : PieceBoard
{
    // Knight moves (L-shape)
    static readonly int[] knightMoves = new int[]
    {
        // Two squares forward, then one left/right
        2 * BitOps.N + 1,  // Up-Left 17
        2 * BitOps.N - 1,  // Up-Right 15 
        -2 * BitOps.N + 1, // Down-Left -15
        -2 * BitOps.N - 1, // Down-Right -17

        // Two squares left/right, then one forward/backward
        1 * BitOps.N + 2,  // Left-Up 10
        1 * BitOps.N - 2,  // Left-Down 6
        -1 * BitOps.N + 2, // Right-Up -6
        -1 * BitOps.N - 2  // Right-Down -10
    };
    public KnightBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'N';
    }

    public KnightBoard(KnightBoard original) : base(original) { }

    public override PieceBoard Clone() => new KnightBoard(this);

    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        foreach (var move in knightMoves)
        {
            int newIndex = index + move;
            if (BitOps.InBounds(newIndex) 
                && BitOps.IsValidKnightMove(index, newIndex) 
                && ((friendBoard & (BitOps.a1 << newIndex)) == 0 || includeFriends)
            ) // Not occupied by friendly piece
            {
                validMoves |= (BitOps.a1 << newIndex); // Add move
            }
        }

        return validMoves; // can only handle 1 index at a time at least for now
    }
}
