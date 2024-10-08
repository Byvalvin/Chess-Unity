using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System;

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
    private GameObject dropdownPrefab;

    public Game Game
    {
        get => game;
        set => game = value;
    }

    private void LoadUIPrefabs()
    {
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
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        // Create Dropdowns
        whitePlayerDropdown = CreateDropdown("WhitePlayerDropdown", Color.cyan);
        blackPlayerDropdown = CreateDropdown("BlackPlayerDropdown", Color.magenta);

        // Create Start Button
        startButton = CreateTMPButton("StartButton", "Start Game");
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Layout
        ArrangeUI();
    }

    private TMP_Dropdown CreateDropdown(string name, Color backgroundColor)
    {
        GameObject dropdownObject = Instantiate(dropdownPrefab);
        dropdownObject.name = name;
        dropdownObject.transform.SetParent(canvas.transform, false);

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogError($"TMP_Dropdown component not found on the instantiated prefab: {name}!");
            return null;
        }

        dropdown.ClearOptions();
        List<string> playerOptions = PlayerTypeUtility.GetPlayerOptions();
        dropdown.AddOptions(playerOptions);
        dropdown.value = 0; // Default to the first option
        dropdown.RefreshShownValue();

        // Set the background color
        Image dropdownImage = dropdownObject.GetComponent<Image>();
        dropdownImage.color = backgroundColor;

        return dropdown;
    }

    private Button CreateTMPButton(string name, string buttonText)
    {
        GameObject buttonObject = new GameObject(name);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        buttonObject.transform.SetParent(canvas.transform, false);
        
        Button button = buttonObject.AddComponent<Button>();
        rectTransform.sizeDelta = new Vector2(140, 35); // Reduced size for the button

        GameObject buttonTextObject = new GameObject("Text");
        buttonTextObject.transform.SetParent(buttonObject.transform);

        TextMeshProUGUI text = buttonTextObject.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 20; // Adjusted font size
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white; // Better contrast

        RectTransform textRectTransform = text.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(140, 35);
        textRectTransform.localPosition = Vector3.zero;

        // Create rounded button background
        Image buttonBackground = buttonObject.AddComponent<Image>();
        buttonBackground.color = Color.gray; // Base color
        buttonBackground.sprite = CreateRoundedSprite(); // Set the rounded sprite

        // Add event triggers for hover effects
        EventTrigger eventTrigger = buttonObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(CreateEventTriggerEntry(() => OnMouseEnter(button), EventTriggerType.PointerEnter));
        eventTrigger.triggers.Add(CreateEventTriggerEntry(() => OnMouseExit(button), EventTriggerType.PointerExit));

        return button;
    }

    private Sprite CreateRoundedSprite()
    {
        // Create a rounded rectangle texture
        Texture2D texture = new Texture2D(1, 1);
        Color[] pixels = new Color[1];
        pixels[0] = Color.gray; // Set a default color
        texture.SetPixels(pixels);
        texture.Apply();

        // Create a new sprite with rounded corners
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private EventTrigger.Entry CreateEventTriggerEntry(Action action, EventTriggerType eventType)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((data) => action());
        return entry;
    }

    private void OnMouseEnter(Button button)
    {
        Image buttonBackground = button.GetComponent<Image>();
        buttonBackground.color = new Color(0.7f, 0.7f, 0.7f); // Change color on hover

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(1.05f, 1.05f, 1f); // Grow slightly
    }

    private void OnMouseExit(Button button)
    {
        Image buttonBackground = button.GetComponent<Image>();
        buttonBackground.color = Color.gray; // Reset color on exit

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one; // Reset scale
    }

    private void ArrangeUI()
    {
        // Set the anchors and position for the dropdowns side by side with spacing
        RectTransform whiteDropdownRect = whitePlayerDropdown.GetComponent<RectTransform>();
        whiteDropdownRect.anchorMin = new Vector2(0.4f, 0.5f);
        whiteDropdownRect.anchorMax = new Vector2(0.4f, 0.5f);
        whiteDropdownRect.anchoredPosition = new Vector2(-60, 0); // Adjusted for spacing

        RectTransform blackDropdownRect = blackPlayerDropdown.GetComponent<RectTransform>();
        blackDropdownRect.anchorMin = new Vector2(0.6f, 0.5f);
        blackDropdownRect.anchorMax = new Vector2(0.6f, 0.5f);
        blackDropdownRect.anchoredPosition = new Vector2(60, 0); // Adjusted for spacing

        // Set the position for the button below the dropdowns
        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -50); // Position it below the dropdowns
    }

    private void OnStartButtonClicked()
    {
        if (whitePlayerDropdown.value < 0 || blackPlayerDropdown.value < 0)
        {
            Debug.LogError("Dropdown values are invalid.");
            return;
        }
        if (game == null)
        {
            Debug.LogError("Game component not found!");
            return;
        }

        string whitePlayerType = whitePlayerDropdown.options[whitePlayerDropdown.value].text;
        string blackPlayerType = blackPlayerDropdown.options[blackPlayerDropdown.value].text;

        Debug.Log($"Selected White Player: {whitePlayerType}, Black Player: {blackPlayerType}");

        game.InitializeGame(whitePlayerType, blackPlayerType);
        
        Destroy(canvas.gameObject);
    }
}
