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
 aKing, Queen, Rook, Bishop and Knight inherit from 
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

    bool MouseUp() => Input.GetMouseButtonUp(0);
    bool MouseDown() => Input.GetMouseButtonDown(0);

    List<Vector2> FilterMoves(Piece piece)
    {
        List<Vector2> filteredValidMoves = new List<Vector2>(),
                    invalidMoves = new List<Vector2>();
        if(piece!=null){
            filteredValidMoves = piece.GetValidMoves();
            // cant move to a square occupied by your own piece
            // cant move to the squares after a piece
            switch(piece.Type){
                case "King":
                    break;
                case "Queen":
                    
                    break;
                case "Rook":
                    
                    break;
                case "Knight":
                    
                    break;
                case "Bishop":
                    break;
                case "Pawn":
                    // can only move diag if there is an opposing piece there
                    foreach (Vector2 pos in filteredValidMoves)
                    {
                        Debug.Log(board.GetTile(pos).piece + " Here");
                        if(Mathf.Abs(piece.Position.x - pos.x) == 1 && 
                            !(board.GetTile(pos).HasPiece() && board.GetTile(pos).piece.Colour!=piece.Colour))
                        {
                            invalidMoves.Add(pos); // Store invalid moves in a separate list
                        }
                    }
                    break;
                default:
                    Debug.Log("Bryhh");
                    break;
            }
        }
        // Now remove the invalid moves from the original list
        foreach (Vector2 invalidMove in invalidMoves)
        {
            filteredValidMoves.Remove(invalidMove);
        }
        return filteredValidMoves;

    }
    Vector2 RoundVector2(Vector2 position)=>new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }
    
    void SelectPiece()
    {
        Vector2 mousePosition = GetMouseWorldPosition();
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
        Vector2 mousePosition = GetMouseWorldPosition();
        if(selectedPiece!=null)
        {
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y,0); // move piece with mouse
            //Debug.Log($"Dragging to: {mousePosition}");
        }
    }
    void ReleasePiece()
    {
        Vector2 mousePosition = GetMouseWorldPosition(),
                targetPosition = RoundVector2(mousePosition / board.TileSize);
        Debug.Log(targetPosition);  
        List<Vector2> validMoves = FilterMoves(selectedPiece);

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
            if(MouseUp())
            {
                ReleasePiece();
            }
        }
    }

    private void HandleInput()
    {
        // Placeholder for player input logic
        // Example: Check for mouse clicks to select and move pieces
        if (MouseDown()) // Left mouse button
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
