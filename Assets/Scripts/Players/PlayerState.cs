using System.Collections.Generic;
using UnityEngine;


public class PlayerState
{
    public string PlayerType { get; set; } // Type of player
    public bool IsWhite { get; set; } // True for white, false for black
    public int TurnIndex{get; private set;} // 0 for white, 1 for black
    public const bool IsTop = false; // IsTop says who is at top of board; true->white at top
    public ulong OccupancyBoard{get; protected set;}

    public char PromoteTo{get; set;}

    public Dictionary<char, PieceBoard> PieceBoards { get; set; } // Dictionary of piece boards

    public bool InCheck{get; set;}
    public bool DoubleCheck{get; set;}
    public bool IsInCheck => InCheck || DoubleCheck;

    public int KingAttacker = -1;

    public PlayerState(string playerType, bool isWhite)
    {
        PlayerType = playerType;
        IsWhite = isWhite;
        TurnIndex = IsWhite?0:1;

        PieceBoards = new Dictionary<char, PieceBoard>
        {
            { 'K', new KingBoard(isWhite, IsTop==isWhite ? (ulong)0x1000000000000000 : (ulong)0x0000000000000010)  }, // King
            { 'Q', new QueenBoard(isWhite, IsTop==isWhite ? (ulong)0x0800000000000000 : (ulong)0x0000000000000008) }, // Queen
            { 'B', new BishopBoard(isWhite, IsTop==isWhite ? (ulong)0x2400000000000000 : (ulong)0x0000000000000024) }, // Bishops
            { 'N', new KnightBoard(isWhite, IsTop==IsWhite ? (ulong)0x4200000000000000 : (ulong)0x0000000000000042) }, // Knights
            { 'R', new RookBoard(isWhite, IsTop==IsWhite ? (ulong)0x8100000000000000 : (ulong)0x0000000000000081) }, // Rooks
            { 'P', new PawnBoard(isWhite, IsTop==IsWhite ? (ulong)0x00FF000000000000 : (ulong)0x000000000000FF00) }  // Pawns
        };

        UpdateOccupancyBoard();
    }
    public PlayerState(PlayerState original){
        PlayerType = original.PlayerType;
        IsWhite = original.IsWhite;
        TurnIndex = original.TurnIndex;

        foreach(char type in original.PieceBoards.Keys)
            PieceBoards[type] = original.PieceBoards[type].Clone();
        
        InCheck = original.InCheck;
        DoubleCheck = original.DoubleCheck;
        KingAttacker = original.KingAttacker;
        
        UpdateOccupancyBoard();
    }
    public PlayerState Clone() => new PlayerState(this);

    public bool RemovePiece(PieceBoard pieceBoard, int index){
        if(PieceBoards[pieceBoard.Type]!=pieceBoard) return false; // not my piece board
        if((pieceBoard.Bitboard & (BitOps.a1 << index)) == 0) return false; // no piece there
        pieceBoard.RemovePiece(index);

        return true; // Successfully removed the piece
    }

    public bool EnPassantChosen(){
        if(PieceBoards['P'] is PawnBoard myPawnBoard)
            return RemovePiece(myPawnBoard, myPawnBoard.enPassantablePawn);
        return false;
    }
    
    
    public void UpdateOccupancyBoard()
    {
        OccupancyBoard = 0UL;
        foreach (var pieceBoard in PieceBoards.Values)
            OccupancyBoard |= pieceBoard.Bitboard; // Combine each piece's bitboard
    }

    public int GetKingIndex(){
        for(int i=0; i<64; i++){
            if((PieceBoards['K'].Bitboard & BitOps.a1<<i)!=0)
                return i;
        }
        return -1;
    }

}
