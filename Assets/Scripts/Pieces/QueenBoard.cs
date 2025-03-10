using System.Collections.Generic;

public class QueenBoard : PieceBoard
{
    public QueenBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'Q';
    }

    public QueenBoard(QueenBoard original) : base(original) { }

    public override PieceBoard Clone() => new QueenBoard(this);

    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Rook-like movements (horizontal and vertical)
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.RightMove, includeFriends, BitOps.MovementType.Rook); // Right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.LeftMove, includeFriends, BitOps.MovementType.Rook);  // Left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.ForwardMove, includeFriends, BitOps.MovementType.Rook); // Up
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.BackwardMove, includeFriends, BitOps.MovementType.Rook); // Down

        // Bishop-like movements (diagonal)
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal1Move, includeFriends, BitOps.MovementType.Bishop); // Up-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal2Move, includeFriends, BitOps.MovementType.Bishop); // Up-right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal3Move, includeFriends, BitOps.MovementType.Bishop); // Down-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal4Move, includeFriends, BitOps.MovementType.Bishop); // Down-right

        return validMoves; // can only handle 1 index at a time at least for now
    }
}
