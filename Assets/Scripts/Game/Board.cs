using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private GameState gameState;
    private const int N = 8; // BOARDSIZE
    private GameObject[,] tiles = new GameObject[N, N]; // Array to hold tile references
    static float tileSize = 5.0f;

    static int sheetN = 1; // The piece sheet we use

    static Dictionary<char, string> pieceTypeMap = new Dictionary<char, string>{
        {'K',"King"},
        {'Q',"Queen"},
        {'R',"Rook"},
        {'B',"Bishop"},
        {'N',"Knight"},
        {'P',"Pawn"}
    };
    static Color[] LightColors = {
        new Color(1f, 0.95f, 0.8f, 1f), // Cream
        new Color(0.9f, 0.9f, 0.9f, 1f), // Very Light Gray
        new Color(1f, 0.94f, 0.8f, 1f), // Soft Beige
        new Color(1f, 1f, 0.8f, 1f), // Soft Yellow
        new Color(0.9f, 0.8f, 1f, 1f), // Light Purple
        new Color(0.8f, 1f, 1f, 1f), // Soft Cyan
        new Color(1f, 0.8f, 0.7f, 1f), // Soft Peach
        new Color(0.8f, 1f, 0.8f, 1f)  // Soft Green
    };
    static Color[] DarkColors = {
        new Color(0.3f, 0.3f, 0.3f, 1f), // Charcoal
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Gray
        new Color(0.4f, 0.26f, 0.2f, 1f), // Soft Brown
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Charcoal
        new Color(0.1f, 0.3f, 0.5f, 1f), // Very Dark Blue
        new Color(0.1f, 0.3f, 0.1f, 1f), // Dark Green
        new Color(0.4f, 0.2f, 0.1f, 1f), // Rich Brown
        new Color(0.4f, 0.4f, 0.5f, 1f)  // Dark Slate Gray
    };
    static int colourIndex = -1; // Will generate same index for all pieces once
    Color lightColour, darkColour;

    static Dictionary<int, float> pieceScaleMap = new Dictionary<int, float>
    {
        { 0, 1.25f },
        { 1, 1.25f },
    };
    float pieceScaleFactor; // Scale factor for pieces
    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(); // Dictionary for sprites

    private void CenterCamera(){
        Camera.main.transform.position = new Vector3((N - 1) * tileSize / 2, (N - 1) * tileSize / 2, -1);
        Camera.main.orthographic = true; // Ensure it's set to Orthographic
        Camera.main.orthographicSize = (N * tileSize) / 2; // Adjust size based on board dimensions
    }

    private void Update()
    {
        // Optionally, you can check for game updates or user input here
        //UpdateBoard(); // Update board visuals based on game state changes
    }

    public void Initialize(GameState state)
    {
        gameState = state;
        UpdateBoard(); // Initialize the board with pieces
    }

    private void LoadSprites(int sheetN = 1)
    {
        // Load all sprites from the Pieces.png
        Sprite[] allSprites = Resources.LoadAll<Sprite>($"Sprites/Pieces{sheetN}"); // Adjust path if needed

        foreach (var sprite in allSprites)
        {
            // Use sprite name directly for the dictionary
            if (!sprites.ContainsKey(sprite.name))
            {
                sprites[sprite.name] = sprite; // Add to dictionary
            }
        }
    }

    private void CreateTiles()
    {
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                // Create a quad for the tile
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(tileSize*x, tileSize*y, 0); // Position the tile
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1); // Scale the tile
                tile.transform.SetParent(transform); // Set parent to keep hierarchy clean

                // Set tile color based on position
                Color tileColor = (x + y) % 2 == (PlayerState.IsTop?0:1) ? Color.white : Color.black;
                tile.GetComponent<Renderer>().material.color = tileColor;

                tiles[x, y] = tile; // Store the tile reference
            }
        }
    }

    public void UpdateBoard()
    {
        // Clear existing pieces
        foreach (Transform child in transform)
        {
            if (child.gameObject.name.EndsWith("_Piece")) // Check if it's a piece
                Destroy(child.gameObject);
        }

        // Logic to update the visual board based on gameState's bitboards
        foreach (var playerState in gameState.PlayerStates)
        {
            foreach (var pieceBoard in playerState.PieceBoards.Values)
            {
                // Check if the piece board has any pieces
                if (pieceBoard.Bitboard == 0) continue;

                // For each piece type, check its bitboard and place the pieces accordingly
                for (int i = 0; i < 64; i++) // Loop through all 64 squares
                {
                    if ((pieceBoard.Bitboard & (1UL << i)) != 0) // Check if the piece is present
                    {
                        int x = i % 8; // X position on the board
                        int y = PlayerState.IsTop && playerState.IsWhite ? 7-(i / 8) : i / 8; // Inverted for black on top; // Y position on the board
                        PlacePiece(pieceBoard.Type, playerState.IsWhite, x, y);
                    }
                }
            }
        }
    }

    private void PlacePiece(char pieceType, bool isWhite, int x, int y)
    {
        // Create a new GameObject for the piece
        GameObject piece = new GameObject($"{pieceType}{(isWhite?'w':'b')}-({x},{y})_Piece");
        piece.transform.position = new Vector3(tileSize*x, tileSize*y, 0); // Place above the tiles

        // Add a SpriteRenderer and assign the correct sprite based on pieceType
        SpriteRenderer spriteRenderer = piece.AddComponent<SpriteRenderer>();
        if (sprites.TryGetValue($"{pieceTypeMap[pieceType]}", out Sprite baseSprite)) // Use char as string for key
        {
            spriteRenderer.sprite = baseSprite; // Set the base piece sprite
            spriteRenderer.color = isWhite ? lightColour : darkColour; // Set color based on piece color
            piece.transform.localScale = new Vector3(tileSize*pieceScaleFactor, tileSize*pieceScaleFactor, 1); // Scale the piece
        }else{
            Debug.LogError("sprite error");
        }

        // Add a BoxCollider2D for the piece
        BoxCollider2D collider = piece.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1/pieceScaleFactor, 1/pieceScaleFactor); // Match collider size to piece size

        //spriteRenderer.sortingOrder = 1; // Ensure pieces are rendered on top
    }




    private void Awake()
    {
        if (colourIndex == -1) // Generate once
            colourIndex = UnityEngine.Random.Range(0, LightColors.Length);
        lightColour = LightColors[colourIndex];
        darkColour = DarkColors[colourIndex];

        // Initialize the piece scale factor
        pieceScaleFactor = pieceScaleMap[sheetN];

        // Create Sprite dictionary
        if (sprites.Count == 0)
            LoadSprites(); // Load sprites only if the dictionary is empty

        CreateTiles(); // Create the visual tiles for the board
    }

    private void Start()
    {
        // Any additional initialization can go here
        CenterCamera();
    }
}
