using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromotionUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panel;

    private System.Action<string> onPromotionSelected;
    // Define the types of pieces that can be promoted to
    string[] pieceTypes = { "Queen", "Rook", "Bishop", "Knight" }; // Customize as needed
    
    // Variable to scale the piece buttons down
    public float pieceScale = 1f; // Adjust this value to scale pieces down

    public void Show(System.Action<string> promotionCallback, Color tileColor, Vector2 tileSize, Piece pawn, Vector2Int tilePosition)
    {
        onPromotionSelected = promotionCallback;

        bool isWhitePlayer = pawn.State.Colour;

        // Create the Canvas
        canvas = new GameObject("PromotionCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; // Set to WorldSpace
        canvas.worldCamera = Camera.main; // Ensure the canvas uses the main camera
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080); // Set a size for the canvas

        // Calculate the world position of the tile
        Vector3 worldPosition = GetWorldPositionFromBoard(tilePosition);

        // Create the Panel
        panel = new GameObject("PromotionPanel");
        panel.transform.SetParent(canvas.transform);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(tileSize.x, tileSize.y * (pieceTypes.Length + 1)); // Adjust for 4 buttons + close button

        // Position the panel above the specified tile
        panel.transform.position = worldPosition; // Adjust height as necessary

        // Add a background image to the panel (optional)
        Image panelImage = panel.AddComponent<Image>();
        //panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent background
        panelImage.color = tileColor; // Semi-transparent background

        // Create buttons dynamically
        for (int i = 0; i < pieceTypes.Length; i++)
        {
            CreateButton(pieceTypes[i], tileColor, pawn.MyColour, tileSize, i);
        }

        // Create and configure the close button
        CreateCloseButton(tileSize);

        panel.SetActive(true);
    }

    private void CreateButton(string pieceType, Color tileColor, Color pieceColor, Vector2 tileSize, int index)
    {
        // Create a button GameObject
        GameObject buttonObject = new GameObject(pieceType + "Button");
        buttonObject.transform.SetParent(panel.transform);

        Button button = buttonObject.AddComponent<Button>();
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = tileSize * pieceScale; // Scale the button size
        rectTransform.anchoredPosition = new Vector2(0, -tileSize.y * index * pieceScale); // Stack vertically with scaling

        // Create the button background image
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = pieceColor;

        // Set the piece image from Board.sprites
        Sprite pieceSprite = Board.sprites[pieceType]; // Access the sprite directly from the dictionary
        buttonImage.sprite = pieceSprite;
        buttonImage.preserveAspect = true; // Keep the aspect ratio

        // Scale the button down
        //buttonObject.transform.localScale = Vector3.one * pieceScale;

        // Add click listener
        button.onClick.AddListener(() => SelectPiece(pieceType));
    }

    private void CreateCloseButton(Vector2 tileSize)
    {
        // Create a close button GameObject
        GameObject closeButtonObject = new GameObject("CloseButton");
        closeButtonObject.transform.SetParent(panel.transform);

        Button closeButton = closeButtonObject.AddComponent<Button>();
        RectTransform rectTransform = closeButtonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(tileSize.x * pieceScale, tileSize.y * pieceScale); // Match tile size with scaling
        rectTransform.anchoredPosition = new Vector2(0, -tileSize.y * pieceTypes.Length * pieceScale); // Position below the other buttons

        // Create the button background image
        Image closeButtonImage = closeButtonObject.AddComponent<Image>();
        closeButtonImage.color = Color.red; // Close button color

        // Optionally add text to the close button using TextMeshPro
        GameObject closeTextObject = new GameObject("CloseText");
        closeTextObject.transform.SetParent(closeButtonObject.transform);
        TextMeshProUGUI closeText = closeTextObject.AddComponent<TextMeshProUGUI>();
        closeText.text = "x";
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        RectTransform closeTextRect = closeTextObject.GetComponent<RectTransform>();
        closeTextRect.sizeDelta = new Vector2(tileSize.x * pieceScale, tileSize.y * pieceScale);
        closeTextRect.anchoredPosition = Vector2.zero; // Center text

        // Add click listener to close the panel
        closeButton.onClick.AddListener(ClosePanel);
    }

    
    private Vector3 GetWorldPositionFromBoard(Vector2Int tilePosition)
    {
        // Assuming each tile is 1 unit in world space
        float tileSize = 5.0f; // Set this to your actual tile size
        return new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0);
    }


    private void SelectPiece(string pieceType)
    {
        onPromotionSelected?.Invoke(pieceType);
        ClosePanel();
    }

    private void ClosePanel()
    {
        Destroy(canvas.gameObject); // Destroy the entire canvas and its contents
    }
}
