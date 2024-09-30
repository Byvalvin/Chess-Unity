using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardState
{
    private TileState[,] tileStates;
    
    private static Vector2Int minPoint, maxPoint;

    public const int n = 8; // Size of the board

    // to change and update state
    public Vector2Int MinPoint{
        get=>minPoint;
        set=>minPoint=value;
    }
    public Vector2Int MaxPoint{
        get=>maxPoint;
        set=>maxPoint=value;
    }
    public int N => n;

    public TileState[,] TileStates{
        get=>tileStates;
        set=>tileStates=value;
    }




    public TileState GetTile(Vector2Int pos)
    {
        int xIndex = pos.x;
        int yIndex = pos.y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < N && 0 <= yIndex && yIndex < N)
        {
            return tileStates[yIndex, xIndex]; // Correct indexing for the array
        }
        return null; // Return null if out of bounds
    }

    public void MovePiece(Vector2Int from, Vector2Int to)
    {
        TileState fromTile = GetTile(from);
        TileState toTile = GetTile(to);

        if (fromTile != null && toTile != null)
        {
            Debug.Log($"Moving piece from {from} to {to}");
            toTile.piece = fromTile.piece;
            fromTile.piece = null;
        }
    }

    // Piece Movement Logic
    public bool InBounds(Vector2Int pos)=>Utility.InBounds(minPoint, maxPoint, pos);

    public void Castle(PieceState king, PieceState rook)
    {
        bool correctTypes = king.Type!="King" && rook.Type!="Rook", sameTeam = king.Colour==rook.Colour, firstMoves = king.FirstMove==rook.FirstMove==true;
        if(correctTypes && sameTeam && firstMoves)
        {
            // castling logic base on side

        }
    }



}

public class Board : MonoBehaviour
{
    // PRIVATE
    BoardState state;
    private Tile[,] tiles;
    private const float tileSize = 5f;
 
    // Load all sprites from the Pieces.png
    static int sheetN = 1; // the piece sheet we use
    static Dictionary<int, float> pieceScaleMap = new Dictionary<int, float>
    {
        { 0, 1.25f },
        { 1, 1.25f },
    };
    float pieceScaleFactor = pieceScaleMap[sheetN]; // increase size of a piece also used to set collider of piece to reciprocal
    static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();


    public float TileSize
    {
        get { return tileSize; }
    }

    public void CreateBoard(Player Player1, Player Player2)
    {
        // Create and Add Tiles
        tiles = new Tile[state.N, state.N];
        state.MinPoint = new Vector2Int(0, 0); state.MaxPoint = new Vector2Int(state.N-1, state.N-1);

        for (int yi = 0; yi < state.N; yi++)
        {
            for (int xi = 0; xi < state.N; xi++)
            {
                GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Tile tile = tileObject.AddComponent<Tile>();

                tile.State.Position = new Vector2Int(xi, yi); // Note the order here
                tile.State.Colour = (yi + xi) % 2 == 1; // Alternate colours
                tile.State.N = tileSize;

                tiles[yi, xi] = tile;

                // Position the tile in the scene
                tileObject.transform.position = new Vector3(xi * tileSize, yi * tileSize, 0);
                tileObject.transform.localScale = new Vector3(tileSize, tileSize, 1); // Flatten for board look
            }
        }

        // need tileSize for bot moves
        Player1.State.TileSize = tileSize; Player2.State.TileSize = tileSize;
        
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
        Sprite[] allSprites = Resources.LoadAll<Sprite>($"Sprites/Pieces{sheetN}"); // Adjust path if needed
        
        foreach (var sprite in allSprites)
        {
            sprites[sprite.name] = sprite; // Map sprite names to the dictionary
        }
    }



    private void PopulateBoard(Player Player1, Player Player2)
    {
        // string[] pieceTypes = { "Pawn", "Bishop", "Knight", "Rook", "Queen", "King" };
        string[] pieceTypes = { "King", "Queen", "Bishop", "Knight", "Rook",  "Pawn" }; // orderd so the King is indeed the first piece in the Player's Pieces List
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
                for (int xi = 0; xi < state.N; xi++)
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
        int darkY = state.MinPoint.y, lightY = state.MaxPoint.y;

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
                darkY++; lightY--;
                piece = PieceObject.AddComponent<Pawn>();
                break;
            default:
                Debug.Log("Unknown piece type: " + type);
                break;
        }

        piece.State.Colour = colour;
        piece.State.Position = new Vector2Int(x, colour ? lightY : darkY);
        piece.State.TileSize = tileSize;
        piece.State.MinPoint = minPoint; 
        piece.State.MaxPoint = maxPoint;
        piece.PieceSprite = sprites[$"{type}"];
        piece.PieceColliderSize = 1 / pieceScaleFactor;

        // Set piece to tile
        int tileY = colour ? lightY : darkY;
        tiles[tileY, x].State.piece = piece; // Adjust for array index
        //Debug.Log(tiles[tileY, x].piece + " "+ tiles[tileY, x].piece.Type + " on tile " + x + " " + tileY);

        // Set UI
        PieceObject.transform.position = new Vector3(x * state.TileSize, tileY * state.TileSize, 0);
        PieceObject.transform.localScale = new Vector3(state.TileSize * pieceScaleFactor, state.TileSize * pieceScaleFactor, 1); // Adjust based on sprite size

        // Give piece to player
        Player.State.AddPiece(piece);
    }


    
    void Awake()
    {
        // create Sprite dict
        if(sprites.Count==0)
            LoadSprites();
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

