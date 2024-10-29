using System.Collections.Generic;
using System;
using UnityEngine;

public class PawnBoard : PieceBoard
{
    
    public int enPassantablePawn;
    public int enPassantCounter;
    public bool canBeCapturedEnPassant => enPassantCounter<=1 && enPassantablePawn!=-1;
    public PawnBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'P';
        enPassantablePawn = -1; // only one pawn can be capped enPassant at a time
        enPassantCounter = 0;
    }

    public PawnBoard(PawnBoard original) : base(original) {
        enPassantablePawn = original.enPassantablePawn;
        enPassantCounter = original.enPassantCounter;
     }

    public override PieceBoard Clone() => new PawnBoard(this);

    // Modify this method to set the en passant target when a pawn moves two squares
    public override void Move(int fromIndex, int toIndex)
    {
        base.Move(fromIndex, toIndex);
        
        // Check if it's a two-square move
        if (Math.Abs(fromIndex - toIndex) == 2 * BitOps.N){ // only possible for first double move
            //enPassantablePawn = toIndex + (IsWhite ? -BitOps.N : BitOps.N); // Set the en passant target
            enPassantablePawn = toIndex;
        }
    }

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
        if (BitOps.IsValidMove(index, forwardIndex, BitOps.MovementType.Pawn) 
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
        if (BitOps.IsValidMove(index, doubleForwardIndex, BitOps.MovementType.Pawn)
            && (friendBoard & (BitOps.a1 << doubleForwardIndex)) == 0
            && (enemyBoard & (BitOps.a1 << doubleForwardIndex)) == 0
        )
        {
            validMoves |= BitOps.a1 << doubleForwardIndex; // Add valid double forward move
        }
    }

    public void AddDiagonalCaptures(ref ulong validMoves, int index, int direction, ulong friendBoard, ulong enemyBoard, bool includeFriends=false)
    {
        int leftCaptureIndex = IsWhite
            ? BitOps.Diagonal1Move(index, direction) // White: up-left
            : BitOps.Diagonal3Move(index, -direction); // Black: down-left

        if (BitOps.IsValidMove(index, leftCaptureIndex, BitOps.MovementType.Pawn) 
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

        if (BitOps.IsValidMove(index, rightCaptureIndex, BitOps.MovementType.Pawn)
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

            if (BitOps.IsValidMove(pawnIndex, leftCaptureIndex, BitOps.MovementType.Pawn)){
                allPawnCaptureMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
            }

            int rightCaptureIndex = IsWhite
                ? BitOps.Diagonal2Move(pawnIndex, direction) // White: up-right
                : BitOps.Diagonal4Move(pawnIndex, -direction); // Black: down-right

            if (BitOps.IsValidMove(pawnIndex, rightCaptureIndex, BitOps.MovementType.Pawn)){
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

        if (BitOps.IsValidMove(pawnIndex, leftCaptureIndex, BitOps.MovementType.Pawn)){
            pawnCaptureMoves |= BitOps.a1 << leftCaptureIndex; // Add left capture move
        }

        int rightCaptureIndex = IsWhite
            ? BitOps.Diagonal2Move(pawnIndex, direction) // White: up-right
            : BitOps.Diagonal4Move(pawnIndex, -direction); // Black: down-right

        if (BitOps.IsValidMove(pawnIndex, rightCaptureIndex, BitOps.MovementType.Pawn)){
            pawnCaptureMoves |= BitOps.a1 << rightCaptureIndex; // Add right capture move
        }
        return pawnCaptureMoves;

    }

    public void AddEnPassant(PawnBoard opponentPawnBoard)
    {
        int direction = IsWhite ? 1 : -1;
        List<(int pawnIndex, ulong enPassantMove)> movesToAdd = new List<(int, ulong)>();

        foreach (int pawnIndex in ValidMovesMap.Keys)
        {
            // Check left en passant
            int leftIndex = IsWhite ? BitOps.Diagonal1Move(pawnIndex, direction) : BitOps.Diagonal3Move(pawnIndex, -direction);
            if (BitOps.IsValidMove(pawnIndex, leftIndex, BitOps.MovementType.Pawn) 
                && (Math.Abs(opponentPawnBoard.enPassantablePawn - leftIndex) == Board.N))
            {
                ulong enPassantMove = BitOps.a1 << leftIndex;
                movesToAdd.Add((pawnIndex, enPassantMove));
            }

            // Check right en passant
            int rightIndex = IsWhite ? BitOps.Diagonal2Move(pawnIndex, direction) : BitOps.Diagonal4Move(pawnIndex, -direction);
            if (BitOps.IsValidMove(pawnIndex, rightIndex, BitOps.MovementType.Pawn) 
                && (Math.Abs(opponentPawnBoard.enPassantablePawn - rightIndex) == Board.N))
            {
                ulong enPassantMove = BitOps.a1 << rightIndex;
                movesToAdd.Add((pawnIndex, enPassantMove));
            }
        }

        // Apply the collected en passant moves
        foreach (var (pawnIndex, enPassantMove) in movesToAdd)
            ValidMovesMap[pawnIndex] |= enPassantMove;
        
    }

    public void EnPassantReset(){
        enPassantCounter = 0; //reset for next pawn
        enPassantablePawn = -1; // Clear target if captured or moved
    }

    public void ResetEnPassant(){
        if (canBeCapturedEnPassant){
            enPassantCounter++;
        }else{
            EnPassantReset();
        }
    }
}
