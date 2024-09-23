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

    Vector2Int originalPosition;


    public void SwitchPlayer()
    {
        currentIndex = (currentIndex + 1) % players.Length;
    }

    HashSet<Vector2Int> GetAllPlayerMoves(int playerIndex)
    {
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach(Piece piece in players[playerIndex].Pieces)
        {
            foreach (Vector2Int move in piece.GetValidMoves())
            {
                if(!(piece.Type=="Pawn" && piece.Position.x==move.x))
                    allMoves.Add(move);
            }
        }
        return allMoves;
    }


    // Piece Movement Logic
    bool FilterPawnMove(Vector2Int pos)
    
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
                sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour==selectedPiece.Colour,
                isDiag = Mathf.Abs(selectedPiece.Position.x - pos.x)==1;

        return (pieceAtpos&&!sameColourPieceAtPos&&isDiag) || (!pieceAtpos&&!isDiag);
    }
    HashSet<Vector2Int> FilterPawnMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> pawnMoves = selectedPiece.GetValidMoves();
        return pawnMoves.FindAll(FilterPawnMove);
    }



    bool FilterKnightMove(Vector2Int pos)
    {
        return false;
    }
    HashSet<Vector2Int> FilterKnightMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> knightMoves = selectedPiece.GetValidMoves();
        return knightMoves.FindAll(FilterKnightMove);
    }



    bool FilterBishopMove(Vector2Int pos)
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour==selectedPiece.Colour;

        // determine line formed by Bishop and pos
        // determine if a piece is on that line
        // if no piece add pos
        // if the pos is "after" the piece and the Bishop, remove pos

        bool onLine1 = pos.x+pos.y == selectedPiece.Position.x+selectedPiece.Position.y;
        int m = onLine1 ? -1:1;
        int x = (int)pos.x, y=(int)pos.y, b = y - m*x;

        HashSet<Vector2Int> pointsBetween = Utility.GetIntermediatePoints(selectedPiece.Position, pos, Utility.MovementType.Diagonal);
        //Debug.Log("InbetweenerslENGNT: "+pointsBetween.Count);
        foreach (Vector2Int apos in pointsBetween)
        {
            //Debug.Log("Inbetweeners: "+apos);
            if(board.GetTile(apos).HasPiece()){
                return false;
            }
            
        }

        return !pieceAtpos || (pieceAtpos && !sameColourPieceAtPos); // leave the move if opposing piece
    }
    HashSet<Vector2Int> FilterBishopMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> bishopMoves = selectedPiece.ValidMoves;
        return bishopMoves.FindAll(FilterBishopMove);
    }



    bool FilterRookMove(Vector2Int pos)
    {
        return false;
    }
    HashSet<Vector2Int> FilterRookMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> rookMoves = selectedPiece.GetValidMoves();
        return rookMoves.FindAll(FilterRookMove);
    }



    bool FilterQueenMove(Vector2Int pos)
    {
        return false;
    }
    HashSet<Vector2Int> FilterQueenMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> queenMoves = selectedPiece.GetValidMoves();
        return queenMoves.FindAll(FilterQueenMove);
    }



    bool FilterKingMove(Vector2Int pos)
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour==selectedPiece.Colour;

        HashSet<Vector2Int> opposingMoves = GetAllPlayerMoves((currentIndex+1)%2);
        
        return !opposingMoves.Contains(pos) && (!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos));
    }
    HashSet<Vector2Int> FilterKingMoves(){
        if(selectedPiece==null){
            return null; // dont even bother
        }
        HashSet<Vector2Int> kingMoves = selectedPiece.GetValidMoves();
        
        return kingMoves.FindAll(FilterKingMove);
    }



    HashSet<Vector2Int> FilterMoves()
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
                if(players[currentIndex].Colour==piece.Colour)// only allow selection for the player to play
                {
                    //Debug.Log($"Selected piece: {piece.Type}");
                    selectedPiece=piece; //Select piece
                    originalPosition = selectedPiece.Position; // Store original position
                }
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

    bool isCapture(Vector2Int targetPosition) => board.GetTile(targetPosition).HasPiece();
    void ReleasePiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / board.TileSize);
        //Debug.Log("target "+targetPosition);  
        HashSet<Vector2Int> validMoves = FilterMoves();

        foreach(Vector2Int move in validMoves)
        {
            Debug.Log(move);
        }
        if(validMoves.Contains(targetPosition))
        {
            if(isCapture(targetPosition))
            {
                Piece captured = board.GetTile(targetPosition).piece;
                players[currentIndex].Capture(captured);
                players[(currentIndex+1)%2].RemovePiece(captured);
                captured.Captured=true;
            }
            
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
