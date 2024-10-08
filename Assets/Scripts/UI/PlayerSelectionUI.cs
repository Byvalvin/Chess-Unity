using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class PlayerSelectionUI : MonoBehaviour
{
    // Essentials
    private Canvas canvas;
    private TMP_Dropdown whitePlayerDropdown;
    private TMP_Dropdown blackPlayerDropdown;
    private Button startButton;
    private Game game;

    // Prefabs
    static bool loadedPrefabs = false;
    private GameObject dropdownPrefab; // To store the prefab

    public Game Game
    {
        get => game;
        set => game = value;
    }

    private void LoadUIPrefabs()
    {
        // Load the prefab from the Resources folder
        dropdownPrefab = Resources.Load<GameObject>("Prefabs/PlayerSelectDD");
    }

    void Awake()
    {
        if (!loadedPrefabs)
        {
            LoadUIPrefabs();
            loadedPrefabs = true;
        }
    }

    void Start()
    {
        CreateUI();
    }

    private void CreateUI()
    {
        // Create Canvas
        canvas = new GameObject("PlayerSelectionCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        // Create Event System if it doesn't exist
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>(); // For mouse input
        }

        // Create Dropdowns
        whitePlayerDropdown = CreateDropdown("WhitePlayerDropdown");
        blackPlayerDropdown = CreateDropdown("BlackPlayerDropdown");

        // Create Start Button
        startButton = CreateTMPButton("StartButton", "Start Game");
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Layout
        ArrangeUI();
    }

    private TMP_Dropdown CreateDropdown(string name)
    {
        // Instantiate the prefab
        GameObject dropdownObject = Instantiate(dropdownPrefab);
        dropdownObject.name = name;
        dropdownObject.transform.SetParent(canvas.transform, false); // Set parent and maintain local scale

        // Get the TMP_Dropdown component directly
        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();

        // If your prefab has a separate GameObject for the template, ensure it's correctly set up
        if (dropdown == null)
        {
            Debug.LogError($"TMP_Dropdown component not found on the instantiated prefab: {name}!");
            return null;
        }

        // Clear any existing options
        dropdown.ClearOptions();

        // Populate dropdown with options
        List<string> playerOptions = PlayerTypeUtility.GetPlayerOptions();
        dropdown.AddOptions(playerOptions);

        dropdown.value = 0; // Default to the first option
        dropdown.RefreshShownValue();

        return dropdown;
    }

    private Button CreateTMPButton(string name, string buttonText)
    {
        GameObject buttonObject = new GameObject(name);

        // Add RectTransform component
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        buttonObject.transform.SetParent(canvas.transform, false);
        
        // Create the Button component
        Button button = buttonObject.AddComponent<Button>();
        rectTransform.sizeDelta = new Vector2(160, 30); // Set size here

        // Create TextMeshPro for the button
        GameObject buttonTextObject = new GameObject("Text");
        buttonTextObject.transform.SetParent(buttonObject.transform);

        TextMeshProUGUI text = buttonTextObject.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

        RectTransform textRectTransform = text.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(160, 30);
        textRectTransform.localPosition = Vector3.zero;

        // Create a background for the button
        Image buttonBackground = buttonObject.AddComponent<Image>();
        buttonBackground.color = Color.gray; // Set background color for visibility

        return button;
    }

    private void ArrangeUI()
    {
        // Set the anchors and position for the dropdowns
        RectTransform whiteDropdownRect = whitePlayerDropdown.GetComponent<RectTransform>();
        whiteDropdownRect.anchorMin = new Vector2(0.5f, 0.5f);
        whiteDropdownRect.anchorMax = new Vector2(0.5f, 0.5f);
        whiteDropdownRect.anchoredPosition = new Vector2(0, 50); // Position it above the black dropdown

        RectTransform blackDropdownRect = blackPlayerDropdown.GetComponent<RectTransform>();
        blackDropdownRect.anchorMin = new Vector2(0.5f, 0.5f);
        blackDropdownRect.anchorMax = new Vector2(0.5f, 0.5f);
        blackDropdownRect.anchoredPosition = new Vector2(0, 0); // Center it

        // Set the position for the button
        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -50); // Position it below the dropdowns
    }

    private void OnStartButtonClicked()
    {
        // Ensure dropdowns have valid selections
        if (whitePlayerDropdown.value < 0 || blackPlayerDropdown.value < 0)
        {
            Debug.LogError("Dropdown values are invalid.");
            return; // Early exit if dropdown values are invalid
        }
        if (game == null)
        {
            Debug.LogError("Game component not found!");
            return;
        }

        // Get selected player types
        string whitePlayerType = whitePlayerDropdown.options[whitePlayerDropdown.value].text;
        string blackPlayerType = blackPlayerDropdown.options[blackPlayerDropdown.value].text;

        Debug.Log($"Selected White Player: {whitePlayerType}, Black Player: {blackPlayerType}");

        // Initialize players in the game
        game.InitializeGame(whitePlayerType, blackPlayerType);
        
        // Optionally destroy the UI after starting the game
        Destroy(canvas.gameObject);
    }
}
