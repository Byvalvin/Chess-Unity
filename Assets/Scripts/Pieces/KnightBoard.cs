using System.Collections.Generic;

public class KnightBoard : PieceBoard
{
    public KnightBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'N';
    }

    public KnightBoard(KnightBoard original) : base(original) { }

    public override PieceBoard Clone() => new KnightBoard(this);

    public override void ResetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Knight moves (L-shape)
        int[] knightMoves = new int[]
        {
            -17, -15, 15, 17,  // Up and Down
            -10, -6, 6, 10     // Left and Right
        };

        foreach (var move in knightMoves)
        {
            int newIndex = index + move;
            if (BitOps.InBounds(newIndex) && ((friendBoard & (BitOps.a1 << newIndex)) == 0 || includeFriends)) // Not occupied by friendly piece
            {
                validMoves |= (BitOps.a1 << newIndex); // Add move
            }
        }

        ValidMovesMap[index] = validMoves; // can only handle 1 index at a time at least for now
    }
}
