using System.Collections.Generic;
using UnityEngine;

public class PawnBoard : PieceBoard
{
    public PawnBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'P';
    }

    public PawnBoard(PawnBoard original) : base(original) { }

    public override PieceBoard Clone() => new PawnBoard(this);

    public override ulong ValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;
        int direction = IsWhite ? 1 : -1;

        // Forward move
        AddForwardMove(ref validMoves, index, direction, friendBoard);

        // First move: two squares
        if (FirstMovers.Contains(index))
        {
            AddDoubleForwardMove(ref validMoves, index, direction, friendBoard);
        }

        // Diagonal captures
        AddDiagonalCaptures(ref validMoves, index, direction, friendBoard, enemyBoard);

        return validMoves;
    }

    private void AddForwardMove(ref ulong validMoves, int index, int direction, ulong friendBoard)
    {
        int forwardIndex = BitOps.ForwardMove(index, direction);
        if (BitOps.InBounds(forwardIndex) && (friendBoard & (BitOps.a1 << forwardIndex)) == 0)
        {
            validMoves |= BitOps.a1 << forwardIndex; // Add valid forward move
        }
    }

    private void AddDoubleForwardMove(ref ulong validMoves, int index, int direction, ulong friendBoard)
    {
        int doubleForwardIndex = BitOps.ForwardMove(index, direction * 2);
        if (BitOps.InBounds(doubleForwardIndex) && (friendBoard & (BitOps.a1 << doubleForwardIndex)) == 0)
        {
            validMoves |= BitOps.a1 << doubleForwardIndex; // Add valid double forward move
        }
    }

    private void AddDiagonalCaptures(ref ulong validMoves, int index, int direction, ulong friendBoard, ulong enemyBoard)
    {
        int leftCaptureIndex = IsWhite
            ? BitOps.Diagonal1Move(index, direction) // White: up-left
            : BitOps.Diagonal3Move(index, -direction); // Black: down-left

        if (BitOps.InBounds(leftCaptureIndex) && (enemyBoard & (BitOps.a1 << leftCaptureIndex)) != 0)
        {
            validMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
        }

        int rightCaptureIndex = IsWhite
            ? BitOps.Diagonal2Move(index, direction) // White: up-right
            : BitOps.Diagonal4Move(index, -direction); // Black: down-right

        if (BitOps.InBounds(rightCaptureIndex) && (enemyBoard & (BitOps.a1 << rightCaptureIndex)) != 0)
        {
            validMoves |= BitOps.a1 << rightCaptureIndex; // Add right capture move
        }
    }
}
