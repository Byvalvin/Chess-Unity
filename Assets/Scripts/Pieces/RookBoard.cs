using System.Collections.Generic;

public class RookBoard : PieceBoard
{
    public RookBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'R';
    }

    public RookBoard(RookBoard original) : base(original) { }

    public override PieceBoard Clone() => new RookBoard(this);

    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Horizontal and vertical movements
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.RightMove, includeFriends, BitOps.MovementType.Rook); // Right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.LeftMove, includeFriends, BitOps.MovementType.Rook);  // Left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.ForwardMove, includeFriends, BitOps.MovementType.Rook); // Up
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.BackwardMove, includeFriends, BitOps.MovementType.Rook); // Down

        return validMoves; // can only handle 1 index at a time at least for now
    }
}
