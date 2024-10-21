using System.Collections.Generic;

public class QueenBoard : PieceBoard
{
    public QueenBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'Q';
    }

    public QueenBoard(QueenBoard original) : base(original) { }

    public override PieceBoard Clone() => new QueenBoard(this);

    public override void ResetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        // Rook-like movements (horizontal and vertical)
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.RightMove, includeFriends); // Right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.LeftMove, includeFriends);  // Left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.ForwardMove, includeFriends); // Up
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.BackwardMove, includeFriends); // Down

        // Bishop-like movements (diagonal)
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal1Move, includeFriends); // Up-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal2Move, includeFriends); // Up-right
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal3Move, includeFriends); // Down-left
        validMoves |= CheckDirection(friendBoard, enemyBoard, index, BitOps.Diagonal4Move, includeFriends); // Down-right

        ValidMovesMap[index] = validMoves; // can only handle 1 index at a time at least for now
    }
}
