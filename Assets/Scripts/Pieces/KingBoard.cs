using System.Collections.Generic;

public class KingBoard : PieceBoard
{
    public KingBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'K';
    }

    public KingBoard(KingBoard original) : base(original) { }

    public override PieceBoard Clone() => new KingBoard(this);

    public override void ResetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // King moves (one square in any direction)
        int[] kingMoves = new int[] { -1, 1, -8, 8, -9, -7, 7, 9 };

        foreach (var move in kingMoves)
        {
            int newIndex = index + move;
            if (BitOps.IsValidMove(index, newIndex))
            {
                ulong newBit = BitOps.a1 << newIndex;
                if ((friendBoard & newBit) == 0 || includeFriends) // Not occupied by friendly piece
                {
                    validMoves |= newBit; // Add move
                }
            }
        }

      ValidMovesMap[index] = validMoves; // can only handle 1 index at a time at least for now
    }
}
