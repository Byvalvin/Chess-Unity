using UnityEngine;
using UnityEngine.UI;

public class PromotionUI : MonoBehaviour
{
    public Button queenButton;
    public Button rookButton;
    public Button bishopButton;
    public Button knightButton;
    public Button closeButton;

    private System.Action<string> onPromotionSelected; // Callback for promotion selection

    void Start()
    {
        // Assign button listeners
        queenButton.onClick.AddListener(() => SelectPromotion("Queen"));
        rookButton.onClick.AddListener(() => SelectPromotion("Rook"));
        bishopButton.onClick.AddListener(() => SelectPromotion("Bishop"));
        knightButton.onClick.AddListener(() => SelectPromotion("Knight"));
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void Show(System.Action<string> promotionCallback)
    {
        onPromotionSelected = promotionCallback;
        gameObject.SetActive(true); // Show the promotion panel
    }

    private void SelectPromotion(string pieceType)
    {
        onPromotionSelected?.Invoke(pieceType); // Call the callback with the selected piece type
        ClosePanel();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false); // Hide the panel
    }
}
