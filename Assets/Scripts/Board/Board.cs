using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardState{
    public event Action<Vector2Int, Vector2Int, bool> OnPieceMoved;
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

    public BoardState(){}
    public BoardState(BoardState original, PlayerState playerState1Clone, PlayerState playerState2Clone){
        // Clone the tile states
        this.tileStates = new TileState[N, N];
        for (int yi = 0; yi < N; yi++)
            for (int xi = 0; xi < N; xi++)
                tileStates[yi, xi] = original.tileStates[yi, xi]?.Clone(); // Clone each tile
        
        foreach (PieceState playerPieceState in playerState1Clone.PieceStates)
            tileStates[playerPieceState.Position.y, playerPieceState.Position.x].pieceState = playerPieceState;
        foreach (PieceState playerPieceState in playerState2Clone.PieceStates)
            tileStates[playerPieceState.Position.y, playerPieceState.Position.x].pieceState = playerPieceState;
    }
    public BoardState Clone(PlayerState playerState1, PlayerState playerState2) => new BoardState(this, playerState1, playerState2); // Clone method

    public void CreateBoardState(PlayerState player1, PlayerState player2){
        // Create and Add Tiles
        tileStates = new TileState[N, N];
        minPoint = new Vector2Int(0, 0); maxPoint = new Vector2Int(N-1, N-1);

        for (int yi = 0; yi < N; yi++){
            for (int xi = 0; xi < N; xi++){
                tileStates[yi, xi] = new TileState((yi + xi) % 2 == 1, minPoint.x, maxPoint.x, new Vector2Int(xi, yi)); //Alt colours
            }
        }
        
        // Create and Add Pieces
        PopulateBoardState(player1, player2);
    }

    private void PopulateBoardState(PlayerState player1, PlayerState player2){
        // string[] pieceTypes = { "Pawn", "Bishop", "Knight", "Rook", "Queen", "King" };
        string[] pieceTypes = { "King", "Queen", "Bishop", "Knight", "Rook",  "Pawn" }; // orderd so the King is indeed the first piece in the Player's Pieces List
        foreach (string pieceType in pieceTypes)
        {
            AddPieceStates(pieceType, player1, player2);
        }
    }

    private void AddPieceStates(string type, PlayerState player1, PlayerState player2){
        switch (type){
            case "King":
                AddPieceState(type, true, 3, player1);
                AddPieceState(type, false, 3, player2);
                break;
            case "Queen":
                AddPieceState(type, true, 4, player1);
                AddPieceState(type, false, 4, player2);
                break;
            case "Rook":
                AddPieceState(type, true, 0, player1);
                AddPieceState(type, true, 7, player1);
                AddPieceState(type, false, 0, player2);
                AddPieceState(type, false, 7, player2);
                break;
            case "Knight":
                AddPieceState(type, true, 1, player1);
                AddPieceState(type, true, 6, player1);
                AddPieceState(type, false, 1, player2);
                AddPieceState(type, false, 6, player2);
                break;
            case "Bishop":
                AddPieceState(type, true, 2, player1);
                AddPieceState(type, true, 5, player1);
                AddPieceState(type, false, 2, player2);
                AddPieceState(type, false, 5, player2);
                break;
            case "Pawn":
                for (int xi = 0; xi < n; xi++)
                {
                    AddPieceState(type, true, xi, player1); // Light
                    AddPieceState(type, false, xi, player2); // Dark
                }
                break;
            default:
                Debug.Log("Unknown piece type: " + type);
                break;
        }
    }

    void AddPieceState(string type, bool colour, int x, PlayerState player){
        int darkY = minPoint.y, lightY = maxPoint.y;
        PieceState pieceState;

        // Handle special case for Pawn
        if (type == "Pawn"){
            darkY++; lightY--;
            pieceState = new PawnState(colour, new Vector2Int(x, colour ? lightY : darkY), minPoint, maxPoint);
        }else{
            Vector2Int startPos = new Vector2Int(x, colour ? lightY : darkY);
            pieceState = Objects.CreatePieceState(type, colour, startPos, minPoint, maxPoint);
        }

        // Set piece to tile
        int tileY = colour ? lightY : darkY;
        tileStates[tileY, x].pieceState = pieceState; // Adjust for array index

        // Give piece to player
        player.AddPieceState(pieceState);
    }


    public TileState GetTile(Vector2Int pos){
        int xIndex = pos.x;
        int yIndex = pos.y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < N && 0 <= yIndex && yIndex < N)
            return tileStates[yIndex, xIndex]; // Correct indexing for the array
        
        return null; // Return null if out of bounds
    }
    public TileState GetTile(int x, int y){ // overload
        int xIndex = x;
        int yIndex = y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < N && 0 <= yIndex && yIndex < N)
            return tileStates[yIndex, xIndex]; // Correct indexing for the array
        
        return null; // Return null if out of bounds
    }

    public void MovePiece(Vector2Int from, Vector2Int to, bool remove=false){
        TileState fromTile = GetTile(from);
        TileState toTile = GetTile(to);

        if (fromTile != null && (toTile != null || remove)){
            //Debug.Log($"Moving piece from {from} to {to} in state");
            if(!remove)toTile.pieceState = fromTile.pieceState;
            fromTile.pieceState = null;
        }
        OnPieceMoved?.Invoke(from, to, remove); // for the board tiles too
    }

    // Piece Movement Logic
    public bool InBounds(Vector2Int pos)=>Utility.InBounds(minPoint, maxPoint, pos);
}

public class Board : MonoBehaviour
{
    // PRIVATE
    BoardState state;
    private Tile[,] tiles;
    private const float tileSize = 5f;
 
    // Load all sprites from the Pieces.png
    static int sheetN = 1; // the piece sheet we use
    static Dictionary<int, float> pieceScaleMap = new Dictionary<int, float>{
        { 0, 1.25f },
        { 1, 1.25f },
    };
    float pieceScaleFactor = pieceScaleMap[sheetN]; // increase size of a piece also used to set collider of piece to reciprocal
    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public float TileSize => tileSize;

    public BoardState State{
        get=>state;
        set{
            state=value;
            state.OnPieceMoved += MovePiece;
        }
    }
    public float PieceScaleFactor => pieceScaleFactor;

    public void CreateBoard(Player Player1, Player Player2){
        // Create and Add Tiles
        tiles = new Tile[state.N, state.N];

        for (int yi = 0; yi < state.N; yi++)
            for (int xi = 0; xi < state.N; xi++){
                tiles[yi, xi] = Objects.CreateTile(xi,yi,tileSize,state.TileStates[yi, xi]);
            }

        // need tileSize for both player and bot moves
        Player1.State.TileSize = tileSize; Player2.State.TileSize = tileSize;
        
        // Create and Add Pieces
        PopulateBoard(Player1, Player2);
    }

    private void CenterCamera(){
        Camera.main.transform.position = new Vector3((state.N - 1) * tileSize / 2, (state.N - 1) * tileSize / 2, -1);
        Camera.main.orthographic = true; // Ensure it's set to Orthographic
        Camera.main.orthographicSize = (state.N * tileSize) / 2; // Adjust size based on board dimensions
    }

    public static Dictionary<string, Sprite> LoadSprites(){
        // Load all sprites from the Pieces.png
        Sprite[] allSprites = Resources.LoadAll<Sprite>($"Sprites/Pieces{sheetN}"); // Adjust path if needed
        
        foreach (var sprite in allSprites)
            sprites[sprite.name] = sprite; // Map sprite names to the dictionary
        return sprites;
    }

    private void PopulateBoard(Player Player1, Player Player2){
        // string[] pieceTypes = { "Pawn", "Bishop", "Knight", "Rook", "Queen", "King" };
        string[] pieceTypes = { "King", "Queen", "Bishop", "Knight", "Rook",  "Pawn" }; // orderd so the King is indeed the first piece in the Player's Pieces List
        foreach (string pieceType in pieceTypes){
            AddPieces(pieceType, Player1, Player2);
        }
    }

    private void AddPieces(string type, Player Player1, Player Player2){
        switch (type){
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
                for (int xi = 0; xi < state.N; xi++){
                    AddPiece(type, true, xi, Player1); // Light
                    AddPiece(type, false, xi, Player2); // Dark
                }
                break;
            default:
                Debug.Log("Unknown piece type: " + type);
                break;
        }
    }

    void AddPiece(string type, bool colour, int x, Player Player){
        int darkY = state.MinPoint.y, lightY = state.MaxPoint.y;
        if(type=="Pawn"){
            darkY++; lightY--;
        }
        int tileY = colour ? lightY : darkY;
        PieceState correspondingPieceState = state.TileStates[tileY, x].pieceState;
        Piece piece = Objects.CreatePiece(
            type + (colour ? "W" : "B") + (type == "Pawn" ? x : ""),
            type, 
            correspondingPieceState, 
            tileSize, 
            pieceScaleFactor
        );

        // Set piece to tile
        tiles[tileY, x].piece = piece; // Adjust for array index
        //Debug.Log(piece);
        //Debug.Log(tiles[tileY, x].piece + " "+ tiles[tileY, x].piece.State.Type + " on tile " + x + " " + tileY);

        // Give piece to player
        Player.AddPiece(piece);
    }


    public Tile GetTile(Vector2Int pos){
        int xIndex = pos.x;
        int yIndex = pos.y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < state.N && 0 <= yIndex && yIndex < state.N)
        {
            return tiles[yIndex, xIndex]; // Correct indexing for the array
        }
        return null; // Return null if out of bounds
    }
    public Tile GetTile(int x, int y){  // overload
        int xIndex = x;
        int yIndex = y; // Invert y coordinate for the array

        if (0 <= xIndex && xIndex < state.N && 0 <= yIndex && yIndex < state.N)
        {
            return tiles[yIndex, xIndex]; // Correct indexing for the array
        }
        return null; // Return null if out of bounds
    }

    public void MovePiece(Vector2Int from, Vector2Int to, bool remove=false){
        Tile fromTile = GetTile(from);
        Tile toTile = GetTile(to);
        if (fromTile != null && (toTile != null || remove))
        {
            Debug.Log($"Moving piece from {from} to {to}");
            if(!remove)toTile.piece = fromTile.piece;
            fromTile.piece = null;
        }
    }
    
    void Awake(){
        // create Sprite dict
        if(sprites.Count==0)
            LoadSprites();
    }
  
    // Start is called before the first frame update
    void Start(){
        CenterCamera();
    }

    // Update is called once per frame
    void Update(){
        // Additional logic if needed
    }

    void OnDestroy() 
    {
        if(state!=null)state.OnPieceMoved -= MovePiece;
    }

}

