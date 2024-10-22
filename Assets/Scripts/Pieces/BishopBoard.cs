using System.Collections.Generic;

public class BishopBoard : PieceBoard
{
    public BishopBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'B';
    }

    public BishopBoard(BishopBoard original) : base(original) { }

    public override PieceBoard Clone() => new BishopBoard(this);

    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Diagonal movements for bishops
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal1Move, includeFriends); // Up-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal2Move, includeFriends); // Up-right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal3Move, includeFriends); // Down-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal4Move, includeFriends); // Down-right

        return validMoves; // can only handle 1 index at a time at least for now
    }
}
