using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromotionUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panel;

    private System.Action<Vector2Int, string> onPromotionSelected;
    // Define the types of pieces that can be promoted to
    string[] pieceTypes = { "Queen", "Rook", "Bishop", "Knight" }; // Customize as needed
    Vector2Int promotionTilePosition;
    
    // Variable to scale the piece buttons down
    public float pieceScale = 1f; // Adjust this value to scale pieces down

    public void Show(System.Action<Vector2Int, string> promotionCallback, Color tileColor, Vector2 tileSize, Color pawnColor, Vector2Int tilePosition){
        onPromotionSelected = promotionCallback;

        promotionTilePosition = tilePosition;

        // Create the Canvas if it doesn't exist
        if (FindObjectOfType<Canvas>() == null){
            canvas = new GameObject("PromotionCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace; 
            canvas.worldCamera = Camera.main; 
            canvas.gameObject.AddComponent<CanvasScaler>(); // Optional: Add scaler for responsiveness
            canvas.gameObject.AddComponent<GraphicRaycaster>(); // Needed for raycasting
        }else{
            canvas = FindObjectOfType<Canvas>(); // Use existing canvas if present
        }
        // Create the Panel
        panel = new GameObject("PromotionPanel");
        panel.transform.SetParent(canvas.transform);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(tileSize.x, tileSize.y * (pieceTypes.Length));

        // Calculate the world position of the tile
        Vector3 worldPosition = GetWorldPositionFromBoard(tilePosition);
        // Determine the position based on tilePosition.y
        if (tilePosition.y == 0){
            // Pawn is at the bottom of the board, position panel above and go up
            panelRect.position = new Vector3(worldPosition.x, worldPosition.y + (panelRect.sizeDelta.y - tileSize.y) / 2, 0);
        }else if (tilePosition.y == 7){
            // Pawn is at the top of the board, position panel below and go down
            panelRect.position = new Vector3(worldPosition.x, worldPosition.y - (panelRect.sizeDelta.y - tileSize.y) / 2, 0);
        }

        // Add a background image to the panel
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = tileColor; // Semi-transparent background

        // Create and configure the close button based on position
        if (tilePosition.y == 0){
            // Create the close button first, at the top
            CreateCloseButton(tileSize, tileColor, true);
            for (int i = 0; i < pieceTypes.Length; i++)
                CreateButton(pieceTypes[i], tileColor, pawnColor, tileSize, i);  
        }
        else if (tilePosition.y == 7){
            for (int i = 0; i < pieceTypes.Length; i++)
                CreateButton(pieceTypes[i], tileColor, pawnColor, tileSize, i);
            // Create the close button last, at the bottom
            CreateCloseButton(tileSize, tileColor, false);
        }

        panel.SetActive(true);
    }


    private void CreateButton(string pieceType, Color tileColor, Color pieceColor, Vector2 tileSize, int index){
        GameObject buttonObject = new GameObject(pieceType + "Button");
        buttonObject.transform.SetParent(panel.transform);

        Button button = buttonObject.AddComponent<Button>();
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = tileSize * pieceScale; // Scale the button size

        // Stack buttons downwards from the top of the panel
        rectTransform.anchoredPosition = new Vector2(0, -(tileSize.y * pieceScale * index) + ((panel.GetComponent<RectTransform>().sizeDelta.y-tileSize.y) / 2)); 

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = pieceColor;

        Sprite pieceSprite = Board.sprites[pieceType]; // Access the sprite directly from the dictionary
        buttonImage.sprite = pieceSprite;
        buttonImage.preserveAspect = true; // Keep the aspect ratio

        button.onClick.AddListener(() => SelectPiece(pieceType));
    }

    private void CreateCloseButton(Vector2 tileSize, Color tileColor, bool isTop){
        // Create a close button GameObject
        GameObject closeButtonObject = new GameObject("CloseButton");
        closeButtonObject.transform.SetParent(panel.transform);

        Button closeButton = closeButtonObject.AddComponent<Button>();
        RectTransform rectTransform = closeButtonObject.AddComponent<RectTransform>();
        // Reduce the button size
        rectTransform.sizeDelta = new Vector2(tileSize.x * pieceScale * 1f, tileSize.y * pieceScale * 1f); // Reduced size

        // Set position based on whether it's at the top or bottom
        if (isTop){
            rectTransform.anchorMin = new Vector2(0.5f, 1); // Anchor to the top center
            rectTransform.anchorMax = new Vector2(0.5f, 1); // Anchor to the top center
            rectTransform.pivot = new Vector2(0.5f, 1); // Pivot to the top
            rectTransform.anchoredPosition = new Vector2(0, rectTransform.sizeDelta.y); // Center it below the top, with offset
        }else{
            rectTransform.anchorMin = new Vector2(0.5f, 0); // Anchor to the bottom center
            rectTransform.anchorMax = new Vector2(0.5f, 0); // Anchor to the bottom center
            rectTransform.pivot = new Vector2(0.5f, 0); // Pivot to the bottom
            rectTransform.anchoredPosition = new Vector2(0, -rectTransform.sizeDelta.y); // Center it above the bottom, with offset
        }

        // Create the button background image
        Image closeButtonImage = closeButtonObject.AddComponent<Image>();
        closeButtonImage.color = tileColor; // Close button color

        // Optionally add text to the close button using TextMeshPro
        GameObject closeTextObject = new GameObject("CloseText");
        closeTextObject.transform.SetParent(closeButtonObject.transform);

        TextMeshProUGUI closeText = closeTextObject.AddComponent<TextMeshProUGUI>();
        // closeText.font = yourBoldFontAsset; // Replace with your actual font asset reference
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.grey;
        closeText.fontSize = 1.5f; // Adjusted font size
        closeText.text = "<b>x</b>"; // Make the text bold using rich text tags
        /*
        closeText.text = "x";
        closeText.fontStyle = FontStyles.Bold; // Set to bold
        */

        RectTransform closeTextRect = closeTextObject.GetComponent<RectTransform>();
        closeTextRect.sizeDelta = rectTransform.sizeDelta; // Match text size to button size
        closeTextRect.anchoredPosition = Vector2.zero; // Center text

        // Add click listener to close the panel
        closeButton.onClick.AddListener(NoSelectPiece);
    }

    private Vector3 GetWorldPositionFromBoard(Vector2Int tilePosition){
        // Assuming each tile is 1 unit in world space
        float tileSize = 5.0f; // Set this to your actual tile size
        return new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0);
    }

    private void SelectPiece(string pieceType){
        Debug.Log("selecting "+pieceType+promotionTilePosition);
        onPromotionSelected?.Invoke(promotionTilePosition, pieceType);
        ClosePanel();
    }
    private void NoSelectPiece(){
        onPromotionSelected?.Invoke(promotionTilePosition, ""); // no slection
        ClosePanel();
    }

    private void ClosePanel(){
        Destroy(canvas.gameObject); // Destroy the entire canvas and its contents
    }
}
