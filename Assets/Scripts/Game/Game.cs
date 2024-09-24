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
        board.currentPlayer = players[currentIndex];
    }



    // Player GUI
    void SelectPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);
        if (collision != null)
        {
            Piece piece = collision.GetComponent<Piece>();
            if (piece != null && players[currentIndex].Colour == piece.Colour) // only allow selection for the player to play
            {
                selectedPiece = piece; //Select piece
                originalPosition = selectedPiece.Position; // Store original position
            }
        }
    }
    void DragPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if (selectedPiece != null)
        {
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0); // move piece with mouse
        }
    }

    bool isCapture(Vector2Int targetPosition) => board.GetTile(targetPosition).HasPiece();
    void ReleasePiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / board.TileSize);
        HashSet<Vector2Int> validMoves = board.FilterMoves(selectedPiece);

        foreach (Vector2Int move in validMoves)
        {
            Debug.Log(move);
        }
        if (validMoves.Contains(targetPosition))
        {
            if (isCapture(targetPosition))
            {
                Piece captured = board.GetTile(targetPosition).piece;
                players[currentIndex].Capture(captured);
                players[(currentIndex + 1) % 2].RemovePiece(captured);
                captured.Captured = true;
            }

            board.MovePiece(selectedPiece.Position, targetPosition);
            selectedPiece.Move(targetPosition);
            SwitchPlayer();
        }
        else
        {
            selectedPiece.Position = originalPosition; // Reset to original position
        }
        selectedPiece = null; // Deselect piece
    }
    void HandleDragAndDrop()
    {
        if (selectedPiece != null)
        {
            DragPiece();
            if (Utility.MouseUp())
            {
                ReleasePiece();
            }
        }
    }

    private void HandleInput()
    {
        if (Utility.MouseDown()) // Left mouse button
        {
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
        Player P1 = gameObject.AddComponent<Player>(), P2 = gameObject.AddComponent<Player>();
        P1.PlayerName = "P1"; P2.PlayerName = "P2";
        P1.Colour = true; P2.Colour = false;

        players[0] = P1;
        players[1] = P2;

        board = gameObject.AddComponent<Board>();
        board.CreateBoard(P1, P2);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
}
