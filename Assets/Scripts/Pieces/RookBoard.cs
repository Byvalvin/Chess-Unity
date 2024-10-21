using System.Collections.Generic;

public class RookBoard : PieceBoard
{
    public RookBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'R';
    }

    public RookBoard(RookBoard original) : base(original) { }

    public override PieceBoard Clone() => new RookBoard(this);

    public override ulong ValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Horizontal and vertical movements
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.RightMove, includeFriends); // Right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.LeftMove, includeFriends);  // Left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.ForwardMove, includeFriends); // Up
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.BackwardMove, includeFriends); // Down

        return validMoves;
    }
}
