using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
There are 11 classes: 
Game, Board, Tile, Player, Piece, King, Queen, Rook, Bishop, Knight,
 Pawn. The Game is composed with Board, 
 The Game is aggregated with Players, 
 The Board is aggregated with Tiles, 
 Tiles may or may not have Pieces, 
 The Player has Pieces and 
 King, Queen, Rook, Bishop and Knight inherit from 
 Piece(an abstract class)
*/
public class Game : MonoBehaviour
{
    private Board board;
    private Player[] players = new Player[2]; // only 2 players for a chess game
    private int currentIndex = 0;

    private Piece selectedPiece = null;

    Vector2 originalPosition;


    public void SwitchPlayer()
    {
        currentIndex = (currentIndex + 1) % players.Length;
    }


    // Piece Movement Logic
    bool FilterPawnMove(Vector2 pos)
    
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
                sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour==selectedPiece.Colour,
                isDiag = Mathf.Abs(selectedPiece.Position.x - pos.x)==1;

        return (pieceAtpos&&!sameColourPieceAtPos&&isDiag) || (!pieceAtpos&&!isDiag);
    }
    List<Vector2> FilterPawnMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> pawnMoves = selectedPiece.GetValidMoves();
        return pawnMoves.FindAll(FilterPawnMove);
    }



    bool FilterKnightMove(Vector2 pos)
    {
        return false;
    }
    List<Vector2> FilterKnightMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> knightMoves = selectedPiece.GetValidMoves();
        return knightMoves.FindAll(FilterKnightMove);
    }



    bool FilterBishopMove(Vector2 pos)
    {
        return false;
    }
    List<Vector2> FilterBishopMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> bishopMoves = selectedPiece.GetValidMoves();
        return bishopMoves.FindAll(FilterBishopMove);
    }



    bool FilterRookMove(Vector2 pos)
    {
        return false;
    }
    List<Vector2> FilterRookMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> rookMoves = selectedPiece.GetValidMoves();
        return rookMoves.FindAll(FilterRookMove);
    }



    bool FilterQueenMove(Vector2 pos)
    {
        return false;
    }
    List<Vector2> FilterQueenMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> queenMoves = selectedPiece.GetValidMoves();
        return queenMoves.FindAll(FilterQueenMove);
    }



    bool FilterKingMove(Vector2 pos)
    {
        return false;
    }
    List<Vector2> FilterKingMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        List<Vector2> kingMoves = selectedPiece.GetValidMoves();
        return kingMoves.FindAll(FilterKingMove);
    }



    List<Vector2> FilterMoves()
    {
        if(selectedPiece==null)
        {
            return null; // no piece was passed
        }
        
        switch(selectedPiece.Type){
            case "King":
                return FilterKingMoves();
            case "Queen":
                return FilterQueenMoves();
            case "Rook":
                return FilterRookMoves();
            case "Knight":
                return FilterKnightMoves();
            case "Bishop":
                return FilterBishopMoves();
            case "Pawn":
                return FilterPawnMoves();
            default:
                Debug.Log("Bryhh");
                return null;
        }
        

    }


    // Player GUI
    void SelectPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);
        if(collision!=null)
        {
            Piece piece = collision.GetComponent<Piece>();
            if(piece!=null)
            {
                //Debug.Log($"Selected piece: {piece.Type}");
                selectedPiece=piece; //Select piece
                originalPosition = selectedPiece.Position; // Store original position
            }
        }
    }
    void DragPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if(selectedPiece!=null)
        {
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y,0); // move piece with mouse
            //Debug.Log($"Dragging to: {mousePosition}");
        }
    }
    void ReleasePiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition(),
                targetPosition = Utility.RoundVector2(mousePosition / board.TileSize);
        Debug.Log("target "+targetPosition);  
        List<Vector2> validMoves = FilterMoves();

        foreach(Vector2 move in validMoves){
            Debug.Log(move);
        }
        if(validMoves.Contains(targetPosition))
        {
            board.MovePiece(selectedPiece.Position, targetPosition);
            selectedPiece.Move(targetPosition);
            SwitchPlayer();
        }
        else{
            selectedPiece.Position = originalPosition; // Reset to original position

        }
        selectedPiece = null; // Deselect piece
    }
    void HandleDragAndDrop()
    {
        if(selectedPiece!=null)
        {
            DragPiece();
            if(Utility.MouseUp())
            {
                ReleasePiece();
            }
        }
    }

    private void HandleInput()
    {
        // Placeholder for player input logic
        // Example: Check for mouse clicks to select and move pieces
        if (Utility.MouseDown()) // Left mouse button
        {
            // Logic to select and move pieces
            // You can call SwitchPlayer() here as needed
            //Debug.Log("handling it");
            SelectPiece();
        }
        else if (selectedPiece != null)
        {
            HandleDragAndDrop();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // add component with gameObject then set the values of that class/componet
        Player P1=gameObject.AddComponent<Player>(), P2=gameObject.AddComponent<Player>();
        P1.PlayerName = "P1"; P2.PlayerName = "P2";
        P1.Colour = true; P2.Colour = false;

        players[0] = P1;
        players[1] = P2;

        board = gameObject.AddComponent<Board>();
        board.CreateBoard(P1,P2);
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
}
