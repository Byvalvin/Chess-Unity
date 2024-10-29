using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    private GameState gameState;


    public const int N = 8; // BOARDSIZE
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
    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(); // Dictionary for sprites


    static Dictionary<int, float> pieceScaleMap = new Dictionary<int, float>
    {
        { 0, 1.25f },
        { 1, 1.25f },
    };
    float pieceScaleFactor; // Scale factor for pieces


    public List<GameObject> WhitePieces{get; set;}
    public List<GameObject> BlackPieces{get; set;}

    Vector2Int originalPosition, originalPromotedPawnPosition;
    GameObject selectedPiece = null, promotedPawn = null;

    // promotion
    private bool isPromotionInProgress = false;

    private void CenterCamera(){
        Camera.main.transform.position = new Vector3((N - 1) * tileSize / 2, (N - 1) * tileSize / 2, -1);
        Camera.main.orthographic = true; // Ensure it's set to Orthographic
        Camera.main.orthographicSize = (N * tileSize) / 2; // Adjust size based on board dimensions
    }

    public void Initialize(GameState state){
        gameState = state;
        
        WhitePieces = new List<GameObject>();
        BlackPieces = new List<GameObject>();
        CreateBoard(); // Initialize the board with pieces

        GameState.OnPieceMoved += UpdateSelectedPieceUI; // auto update ui for bord after piece moves
        LogBoard();
        //PieceBoard.PrintBitboard(gameState.OccupancyBoard);
    }

    public void CreateBoard(){
        // Logic to update the visual board based on gameState's bitboards
        foreach (var playerState in gameState.PlayerStates){
            foreach (var pieceBoard in playerState.PieceBoards.Values){
                // Check if the piece board has any pieces
                if (pieceBoard.Bitboard == 0) continue;

                // For each piece type, check its bitboard and place the pieces accordingly
                for (int i = 0; i < 64; i++){// Loop through all 64 squares
                    if ((pieceBoard.Bitboard & (BitOps.a1 << i)) != 0){ // Check if the piece is present
                        int x = i % 8; // X position on the board
                        int y = i / 8; // 7 - (i / 8)
                        GameObject piece = CreatePiece(pieceBoard.Type, playerState.IsWhite);
                        SetPosition(piece, x, y);

                        // "give" piece to player for reference
                        if(playerState.IsWhite)
                            WhitePieces.Add(piece);
                        else
                            BlackPieces.Add(piece);
                    }
                }
            }
        }
    }
    private string GetPieceAtIndex(int index){
        foreach (var playerState in gameState.PlayerStates)
            foreach (var pieceBoard in playerState.PieceBoards.Values)
                if ((pieceBoard.Bitboard & (BitOps.a1 << index)) != 0)
                    return $"{pieceBoard.Type}{(pieceBoard.IsWhite ? 'w' : 'b')}";
        return " o "; // Return a dot for empty squares
    }
    public void LogBoard(){
        Debug.Log("Current Board State:");
        
        for (int y = 7; y >= 0; y--){ // From 1st rank to 8th rank
            string row = $"Row {y}: "; // Adjust for logging
            for (int x = 0; x < 8; x++) {// From a-file to h-file
                int index = BitOps.GetIndex(y, x); // Calculate the index
                string pieceChar = GetPieceAtIndex(index);
                //row += pieceChar + "" + index + " "; // Add the piece character to the row
                row += pieceChar + " "; // Add the piece character to the row
            }
            Debug.Log(row); // Log the row
        }
    }

    private Dictionary<string, Sprite> LoadSprites(int sheetN = 1){
        // Load all sprites from the Pieces.png
        Sprite[] allSprites = Resources.LoadAll<Sprite>($"Sprites/Pieces{sheetN}"); // Adjust path if needed

        foreach (var sprite in allSprites)
            if (!sprites.ContainsKey(sprite.name)) // Use sprite name directly for the dictionary
                sprites[sprite.name] = sprite; // Add to dictionary
        return sprites;
    }

    private void CreateTiles(){
        for (int y = 0; y < N; y++){
            for (int x = 0; x < N; x++){
                // Create a quad for the tile
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(tileSize*x, tileSize*y, 0); // Position the tile
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1); // Scale the tile
                tile.transform.SetParent(transform); // Set parent to keep hierarchy clean

                // Set tile color based on position
                Color tileColor = (x + y) % 2 == 1 ? Color.white : Color.black;
                tile.GetComponent<Renderer>().material.color = tileColor;

                tiles[x, y] = tile; // Store the tile reference
            }
        }
    }


    private GameObject CreatePiece(char pieceType, bool isWhite){
        // Create a new GameObject for the piece
        GameObject piece = new GameObject($"{pieceType}{(isWhite?'w':'b')}_Piece");

        // Add a SpriteRenderer and assign the correct sprite based on pieceType
        SpriteRenderer spriteRenderer = piece.AddComponent<SpriteRenderer>();
        if (sprites.TryGetValue($"{pieceTypeMap[pieceType]}", out Sprite baseSprite)){// Use char as string for key
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
        return piece;
    }
    GameObject GetTile(Vector2Int promotionTileLocation)=>tiles[promotionTileLocation.x, promotionTileLocation.y];

    
    public bool InBounds(Vector2Int position) => Utility.InBounds(position, 0, Board.N);
    Vector2Int GetIndexPosition(Vector2 pos)=>Utility.RoundVector2(pos/tileSize);
    bool ValidTargetPosition(Vector2Int targetPosition)=>InBounds(targetPosition);
    private void SetPosition(GameObject piece, int x, int y)=>piece.transform.position = new Vector3(tileSize*x, tileSize*y, 0);
    private void SetPosition(GameObject piece, Vector2Int position)=> SetPosition(piece, position.x, position.y);
    private void UpdateSelectedPieceUI(Vector2Int finalPosition, bool isCaptureMove=false, Vector2Int removedPiecePosition=default, bool isCastleMove=false, Vector2Int castledRookPosition=default, bool isPromotion=false){
        //ui update

        // captures ui update
        if(isCaptureMove){
            List<GameObject> pieces = gameState.currentIndex==0?BlackPieces:WhitePieces;
            GameObject targetPiece = pieces.FirstOrDefault(p => 
                GetIndexPosition(p.transform.position) == removedPiecePosition); // Check for any piece at the target position
            if (targetPiece != null)
                SetPosition(targetPiece, PieceBoard.purgatory);
        }

        //castle ui update
        if(isCastleMove){ // moves the rook for castles since two pieces move
            List<GameObject> pieces = gameState.currentIndex==0?WhitePieces:BlackPieces;
            GameObject targetPiece = pieces.FirstOrDefault(p => 
                GetIndexPosition(p.transform.position) == castledRookPosition); // Check for any piece at the target position
            if (targetPiece != null)
                SetPosition(targetPiece, finalPosition);
        }else{ // move the selected piece
            if(isPromotion){
                bool forWhite = gameState.PlayerStates[gameState.currentIndex].IsWhite;
                // remove promotePawn
                SetPosition(promotedPawn, PieceBoard.heavenORhell);
                promotedPawn = null; // reset promoted pawn

                // add new piece
                GameObject replacementPiece = CreatePiece(gameState.PromoteTo, forWhite);
                SetPosition(replacementPiece, finalPosition);
                List<GameObject> pieces = forWhite? WhitePieces:BlackPieces;
                pieces.Add(replacementPiece);

            }else{
                SetPosition(selectedPiece, finalPosition);
            }
        }
        
        // reset piece selection
        DeselectPiece();
    }


    void DeselectPiece() { // Reset selected piece
        selectedPiece = null;
        originalPosition = default;
    }
    void SelectPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);

        bool hit = collision != null, // Check if the piece is present in the bitboard) // Confirm the piece's color matches the player's color
            pieceObjectFound=hit && collision.gameObject.name.EndsWith("_Piece");

        if(hit && pieceObjectFound){ 
            selectedPiece = collision.gameObject;
            originalPosition = GetIndexPosition(collision.gameObject.transform.position);
            int index = BitOps.GetIndex(originalPosition);
            
            if((gameState.OccupancyBoard & (BitOps.a1 << index)) == 0
            || gameState.PlayerStates[gameState.currentIndex].IsWhite != (selectedPiece.name[1]=='w')
            ){
                DeselectPiece(); //reset piece selection
                Debug.Log("No piece at pos OR selected piece is NOT the same colour as player to play");
            }
        }else{
            Debug.Log("Collision null OR no gameobject with name foud");
        }
        // if(selectedPiece!=null){
        //     Debug.Log("selectedPiece = "+selectedPiece);

        // }else{
        //     Debug.Log("selectedPiece = null");
        // }
    }

    void DragPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if (selectedPiece != null)
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0); // move piece with mouse
    }

    void LogMoves(){
                /*
        Debug.Log(pieceBoard.Type + " " + (pieceBoard.IsWhite?"w":"b") + originalIndex + " " + index + "moves");
        for(int i=0; i<64; i++){
            if((pieceBoard.ValidMovesMap[originalIndex]&BitOps.a1<<i)!=0)
                Debug.Log(i);
        }
        Debug.Log("-------------------");
        for(int i=0; i<64; i++){
            if((pieceMoves&BitOps.a1<<i)!=0)
                Debug.Log(i);
        }
        */
    }
    public void ShowPromotionOptions(PieceBoard pieceBoard, Vector2Int promotionPosition){
        PromotionUI promotionUI = gameObject.AddComponent<PromotionUI>();
        GameObject promotionTile = GetTile(promotionPosition);
        promotionUI.Show(OnPromotionSelected,
                        promotionPosition,
                        new Vector2(tileSize,tileSize),
                        promotionTile.GetComponent<Renderer>().material.color,
                        selectedPiece.GetComponent<SpriteRenderer>().color,
                        pieceBoard
        );
    }
    private void OnPromotionSelected(Vector2Int targetPosition, char pieceType, PieceBoard promotedPawnBoard){
        // Update the promoteTo variable in GameState
        gameState.PlayerStates[gameState.currentIndex].PromoteTo = pieceType; // for bot vs player
        gameState.PromoteTo = pieceType;
        if(gameState.PromoteTo!='\0'){
            int index = BitOps.GetIndex(targetPosition),
                originalIndex = BitOps.GetIndex(originalPromotedPawnPosition);
            //PieceBoard pieceBoard = gameState.PlayerStates[gameState.currentIndex].PieceBoards[selectedPiece.name[0]];
            PieceBoard pieceBoard = promotedPawnBoard;
            gameState.ExecuteMove(pieceBoard, originalIndex, index);
        }else{
            // Reset to original position if the move wasnt made
            UpdateSelectedPieceUI(originalPosition);
        }
        // Reset the promotion state
        isPromotionInProgress = false; // Reset after promotion is handled
    }
    void ReleasePiece(){
        Vector2Int targetPosition = GetIndexPosition(Utility.GetMouseWorldPosition());
        int index = BitOps.GetIndex(targetPosition);
        int originalIndex = BitOps.GetIndex(originalPosition);

        Debug.Log($"Attempting to move from {originalPosition} (Index: {originalIndex}) to {targetPosition} (Index: {index})");

        // Check if the target position is valid (i.e., within bounds and not occupied by the player's own piece)
        PieceBoard pieceBoard = gameState.PlayerStates[gameState.currentIndex].PieceBoards[selectedPiece.name[0]];
        //ulong pieceMoves = pieceBoard.ValidMovesMap[originalIndex];
        ulong pieceMoves = gameState.GetMovesAllowed(pieceBoard, originalIndex);

        bool canMove = ValidTargetPosition(targetPosition) 
                    && pieceBoard.CanMove(originalIndex, index) 
                    && (pieceMoves & BitOps.a1<<index)!=0;

        if (canMove){
            if(GameState.IsPromotion(pieceBoard, index)){
                //show promotion ui
                isPromotionInProgress = true;
                promotedPawn = selectedPiece;
                originalPromotedPawnPosition = originalPosition;
                ShowPromotionOptions(pieceBoard, targetPosition);

            }else{
                // Execute the move
                gameState.ExecuteMove(pieceBoard, originalIndex, index);
                //uses action in GameState to auto update ui, Board is a listeneer
                Debug.Log($"Moved piece to {targetPosition} (Index: {index})");
                Debug.Log(gameState.PlayerStates[1-gameState.currentIndex].InCheck+" king check");
            }
        } else{
            // Reset to original position if the move isn't valid
            UpdateSelectedPieceUI(originalPosition);
            Debug.Log("Invalid move, resetting position.");
        }
        
        // Log the board state after the move
        //LogBoard();
        //PieceBoard.PrintBitboard(gameState.OccupancyBoard);
    }


    void HandleDragAndDrop(){
        if (selectedPiece != null){
            DragPiece();
            if (Utility.MouseUp())
                ReleasePiece();
        }
    }
    
    void HandleInput(){
        if(gameState.Gameover) return; // dont handl user input

        if (Utility.MouseDown()) // Left mouse button
            SelectPiece();
        else if (selectedPiece != null)
            if(!isPromotionInProgress) // can use drag and drop while promoting
                HandleDragAndDrop();
    }



    private void Awake(){
        if (colourIndex == -1) // Generate once
            colourIndex = UnityEngine.Random.Range(0, LightColors.Length);
        lightColour = LightColors[colourIndex]; darkColour = DarkColors[colourIndex];

        // Initialize the piece scale factor
        pieceScaleFactor = pieceScaleMap[sheetN];

        // Create Sprite dictionary
        if (sprites.Count == 0)
            LoadSprites(); // Load sprites only if the dictionary is empty

        CreateTiles(); // Create the visual tiles for the board
    }

    private void Start(){
        // Any additional initialization can go here
        CenterCamera();
    }

    private void Update(){
        HandleInput();
    }
}
