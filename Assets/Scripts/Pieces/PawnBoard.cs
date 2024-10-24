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

    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;
        int direction = IsWhite ? 1 : -1;

        // Forward move
        bool canMoveFwd = AddForwardMove(ref validMoves, index, direction, friendBoard, enemyBoard); // cant double fwd if can mvoe forward

        // First move: two squares
        if (canMoveFwd && FirstMovers.Contains(index))
        {
            AddDoubleForwardMove(ref validMoves, index, direction, friendBoard, enemyBoard);
        }

        // Diagonal captures
        AddDiagonalCaptures(ref validMoves, index, direction, friendBoard, enemyBoard);

        return validMoves; // can only handle 1 index at a time at least for now
    }

    private bool AddForwardMove(ref ulong validMoves, int index, int direction, ulong friendBoard, ulong enemyBoard)
    {
        int forwardIndex = BitOps.ForwardMove(index, direction);
        if (BitOps.IsValidMove(index, forwardIndex) 
            && (friendBoard & (BitOps.a1 << forwardIndex)) == 0
            && (enemyBoard & (BitOps.a1 << forwardIndex)) == 0
        )
        {
            validMoves |= BitOps.a1 << forwardIndex; // Add valid forward move
            return true;
        }
        return false;
    }

    private void AddDoubleForwardMove(ref ulong validMoves, int index, int direction, ulong friendBoard, ulong enemyBoard)
    {
        int doubleForwardIndex = BitOps.ForwardMove(index, direction * 2);
        if (BitOps.IsValidMove(index, doubleForwardIndex)
            && (friendBoard & (BitOps.a1 << doubleForwardIndex)) == 0
            && (enemyBoard & (BitOps.a1 << doubleForwardIndex)) == 0
        )
        {
            validMoves |= BitOps.a1 << doubleForwardIndex; // Add valid double forward move
        }
    }

    private void AddDiagonalCaptures(ref ulong validMoves, int index, int direction, ulong friendBoard, ulong enemyBoard, bool includeFriends=false)
    {
        int leftCaptureIndex = IsWhite
            ? BitOps.Diagonal1Move(index, direction) // White: up-left
            : BitOps.Diagonal3Move(index, -direction); // Black: down-left

        if (BitOps.IsValidMove(index, leftCaptureIndex) 
            && ( (enemyBoard & (BitOps.a1 << leftCaptureIndex)) != 0
                || ( 
                    (friendBoard & (BitOps.a1 << leftCaptureIndex))!=0 && includeFriends
                )
            )
        )
        {
            validMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
        }

        int rightCaptureIndex = IsWhite
            ? BitOps.Diagonal2Move(index, direction) // White: up-right
            : BitOps.Diagonal4Move(index, -direction); // Black: down-right

        if (BitOps.IsValidMove(index, rightCaptureIndex)
            && ( (enemyBoard & (BitOps.a1 << rightCaptureIndex)) != 0
                || (
                    (friendBoard & (BitOps.a1 << rightCaptureIndex))!=0 && includeFriends
                )
            )
        )
        {
            validMoves |= BitOps.a1 << rightCaptureIndex; // Add right capture move
        }
    }

    public ulong GetForwardMoves(ulong friendBoard, ulong enemyBoard){
        ulong allPawnForwardMoves = 0UL;
        int direction = IsWhite ? 1 : -1;
        
        foreach (int pawnIndex in ValidMovesMap.Keys)
        {
            // Forward move
            bool canMoveFwd = AddForwardMove(ref allPawnForwardMoves, pawnIndex, direction, friendBoard, enemyBoard); // cant double fwd if can mvoe forward

            // First move: two squares
            if (canMoveFwd && FirstMovers.Contains(pawnIndex))
            {
                AddDoubleForwardMove(ref allPawnForwardMoves, pawnIndex, direction, friendBoard, enemyBoard);
            }
        }
        return allPawnForwardMoves;
    }
    public ulong GetAttackMoves(){
        ulong allPawnCaptureMoves = 0UL;
        int direction = IsWhite ? 1 : -1;
        
        foreach (int pawnIndex in ValidMovesMap.Keys){
            int leftCaptureIndex = IsWhite
                ? BitOps.Diagonal1Move(pawnIndex, direction) // White: up-left
                : BitOps.Diagonal3Move(pawnIndex, -direction); // Black: down-left

            if (BitOps.IsValidMove(pawnIndex, leftCaptureIndex)){
                allPawnCaptureMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
            }

            int rightCaptureIndex = IsWhite
                ? BitOps.Diagonal2Move(pawnIndex, direction) // White: up-right
                : BitOps.Diagonal4Move(pawnIndex, -direction); // Black: down-right

            if (BitOps.IsValidMove(pawnIndex, rightCaptureIndex)){
                allPawnCaptureMoves |= BitOps.a1 << rightCaptureIndex; // Add right capture move
            }
        }
        return allPawnCaptureMoves;
    }

    public ulong GetAttackMove(int pawnIndex){
        ulong pawnCaptureMoves = 0UL;
        int direction = IsWhite ? 1 : -1;
        int leftCaptureIndex = IsWhite
            ? BitOps.Diagonal1Move(pawnIndex, direction) // White: up-left
            : BitOps.Diagonal3Move(pawnIndex, -direction); // Black: down-left

        if (BitOps.IsValidMove(pawnIndex, leftCaptureIndex)){
            pawnCaptureMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
        }

        int rightCaptureIndex = IsWhite
            ? BitOps.Diagonal2Move(pawnIndex, direction) // White: up-right
            : BitOps.Diagonal4Move(pawnIndex, -direction); // Black: down-right

        if (BitOps.IsValidMove(pawnIndex, rightCaptureIndex)){
            pawnCaptureMoves |= BitOps.a1 << rightCaptureIndex; // Add right capture move
        }
        return pawnCaptureMoves;

    }
}
