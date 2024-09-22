using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // PRIVATE
    private Tile[,] tiles;

    private Vector2 minPoint, maxPoint;
    private const float tileSize = 5f;
    float pieceScaleFactor = 1.25f; // increase size of a piece also used to set collider of piece to reciprocal

    // Load all sprites from the Pieces.png
    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public const int N = 8; // Size of the board

    public void CreateBoard(Player Player1, Player Player2)
    {
        // create Sprite dict
        LoadSprites();

        // Create and Add Tiles
        tiles = new Tile[N, N];
        minPoint = new Vector2(0, 0); maxPoint = new Vector2(N-1, N-1);

        for (int yi = 0; yi < N; yi++)
        {
            for (int xi = 0; xi < N; xi++)
            {
                GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Tile tile = tileObject.AddComponent<Tile>();

                tile.Position = new Vector2(xi, yi); // Note the order here
                tile.Colour = (yi + xi) % 2 == 1; // Alternate colours
                tile.N = tileSize;

                tiles[yi, xi] = tile;

                // Position the tile in the scene
                tileObject.transform.position = new Vector3(xi * tileSize, yi * tileSize, 0);
                tileObject.transform.localScale = new Vector3(tileSize, tileSize, 1); // Flatten for board look
            }
        }

        // Create and Add Pieces
        PopulateBoard(Player1, Player2);
    }

    private void CenterCamera()
    {
        Camera.main.transform.position = new Vector3((N - 1) * tileSize / 2, (N - 1) * tileSize / 2, -10);
        Camera.main.orthographic = true; // Ensure it's set to Orthographic
        Camera.main.orthographicSize = (N * tileSize) / 2; // Adjust size based on board dimensions
    }

    private void LoadSprites()
    {
        // Load all sprites from the Pieces.png
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites/Pieces"); // Adjust path if needed
        
        foreach (var sprite in allSprites)
        {
            sprites[sprite.name] = sprite; // Map sprite names to the dictionary
        }
    }

    public float TileSize
    {
        get { return tileSize; }
    }

    public Tile GetTile(Vector2 pos)
    {
        int xIndex = (int)pos.x;
        int yIndex = (int)pos.y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < N && 0 <= yIndex && yIndex < N)
        {
            return tiles[yIndex, xIndex]; // Correct indexing for the array
        }
        return null; // Return null if out of bounds
    }

    public void MovePiece(Vector2 from, Vector2 to)
    {
        Tile fromTile = GetTile(from);
        Tile toTile = GetTile(to);

        if (fromTile != null && toTile != null)
        {
            Debug.Log($"Moving piece from {from} to {to}");
            toTile.piece = fromTile.piece;
            fromTile.piece = null;
        }
    }

    private void PopulateBoard(Player Player1, Player Player2)
    {
        string[] pieceTypes = { "Pawn", "Bishop", "Knight", "Rook", "Queen", "King" };
        foreach (string pieceType in pieceTypes)
        {
            AddPieces(pieceType, Player1, Player2);
        }
    }

    private void AddPieces(string type, Player Player1, Player Player2)
    {
        switch (type)
        {
            case "King":
                AddPiece(type, true, 3, Player1);
                AddPiece(type, false, 3, Player2);
                break;
            case "Queen":
                AddPiece(type, true, 4, Player1);
                AddPiece(type, false, 4, Player2);
                break;
            case "Rook":
                AddPiece(type, true, 0, Player1);
                AddPiece(type, true, 7, Player1);
                AddPiece(type, false, 0, Player2);
                AddPiece(type, false, 7, Player2);
                break;
            case "Knight":
                AddPiece(type, true, 1, Player1);
                AddPiece(type, true, 6, Player1);
                AddPiece(type, false, 1, Player2);
                AddPiece(type, false, 6, Player2);
                break;
            case "Bishop":
                AddPiece(type, true, 2, Player1);
                AddPiece(type, true, 5, Player1);
                AddPiece(type, false, 2, Player2);
                AddPiece(type, false, 5, Player2);
                break;
            case "Pawn":
                for (int xi = 0; xi < N; xi++)
                {
                    AddPiece(type, true, xi, Player1); // Light
                    AddPiece(type, false, xi, Player2); // Dark
                }
                break;
            default:
                Debug.Log("Unknown piece type: " + type);
                break;
        }
    }

    void AddPiece(string type, bool colour, int x, Player Player)
    {
        int darkY = (int)minPoint.y, lightY = (int)maxPoint.y;

        GameObject PieceObject = new GameObject(type + (colour ? "W" : "B") + (type == "Pawn" ? x : ""));
        Piece piece = null;
        switch (type)
        {
            case "King":
                piece = PieceObject.AddComponent<King>();
                break;
            case "Queen":
                piece = PieceObject.AddComponent<Queen>();
                break;
            case "Rook":
                piece = PieceObject.AddComponent<Rook>();
                break;
            case "Knight":
                piece = PieceObject.AddComponent<Knight>();
                break;
            case "Bishop":
                piece = PieceObject.AddComponent<Bishop>();
                break;
            case "Pawn":
                darkY++;
                lightY--;
                piece = PieceObject.AddComponent<Pawn>();
                break;
            default:
                Debug.Log("Unknown piece type: " + type);
                break;
        }

        piece.Colour = colour;
        piece.Position = new Vector2(x, colour ? lightY : darkY);
        piece.TileSize = tileSize;
        piece.MinPoint = minPoint; 
        piece.MaxPoint = maxPoint;
        piece.PieceSprite = sprites[$"{type}"];
        piece.PieceColliderSize = 1 / pieceScaleFactor;

        // Set piece to tile
        int tileY = colour ? lightY : darkY;
        tiles[tileY, x].piece = piece; // Adjust for array index
        Debug.Log(tiles[tileY, x].piece + " "+ tiles[tileY, x].piece.Type + " on tile " + x + " " + tileY);

        // Set UI
        PieceObject.transform.position = new Vector3(x * tileSize, tileY * tileSize, 0);
        PieceObject.transform.localScale = new Vector3(tileSize * pieceScaleFactor, tileSize * pieceScaleFactor, 1); // Adjust based on sprite size

        // Give piece to player
        Player.AddPiece(piece);
    }

    // Start is called before the first frame update
    void Start()
    {
        CenterCamera();
    }

    // Update is called once per frame
    void Update()
    {
        // Additional logic if needed
    }
}
