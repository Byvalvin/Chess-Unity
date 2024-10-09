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
    private TMP_InputField whitePlayerNameInput;
    private TMP_InputField blackPlayerNameInput;

    private Button startButton;
    private Game game;

    // Prefabs
    static bool loadedPrefabs = false;
    private GameObject dropdownPrefab, inputFieldPrefab; // To store the reference to your prefab;

    // design
    Color whiteUIcolour = Color.cyan,  blackUIcolour = Color.magenta;
    public Game Game{
        get => game;
        set => game = value;
    }

    private void LoadUIPrefabs(){
        dropdownPrefab = Resources.Load<GameObject>("Prefabs/PlayerSelectDD");
        inputFieldPrefab = Resources.Load<GameObject>("Prefabs/PlayerSelectInput");
    }

    void Awake(){
        if (!loadedPrefabs){
            LoadUIPrefabs();
            loadedPrefabs = true;
        }
    }

    void Start(){
        CreateUI();
    }

    private void CreateUI(){
        // Create Canvas
        canvas = new GameObject("PlayerSelectionCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        // Create Event System if it doesn't exist
        if (FindObjectOfType<EventSystem>() == null){
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        // Load the image sprite
        string choice = "Queen";
        Sprite logoSprite = Board.LoadSprites()[choice]; // Update this path to your image

        // Create the image at the top center
        CreateImage("GameLogo", logoSprite);

        // Create Dropdowns
        whitePlayerDropdown = CreateDropdown("WhitePlayerDropdown", whiteUIcolour);
        blackPlayerDropdown = CreateDropdown("BlackPlayerDropdown", blackUIcolour);
        // Add listeners to dropdowns
        whitePlayerDropdown.onValueChanged.AddListener(OnWhitePlayerTypeChanged);
        blackPlayerDropdown.onValueChanged.AddListener(OnBlackPlayerTypeChanged);

        // Create Player Name Input Fields
        whitePlayerNameInput = CreateInputField("WhitePlayerNameInput", "P1", whiteUIcolour);
        blackPlayerNameInput = CreateInputField("BlackPlayerNameInput", "P2", blackUIcolour);

        // Create Start Button
        startButton = CreateTMPButton("StartButton", "Start Game");
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Layout
        ArrangeUI();
    }

    private void OnWhitePlayerTypeChanged(int index){
        string selectedPlayerType = whitePlayerDropdown.options[index].text;

        // Update the input field based on the selected player type
        whitePlayerNameInput.text = selectedPlayerType == "Player"? "P1" : selectedPlayerType; // Set the name to the player type

        // Check if player types are the same and update black player's name accordingly
        if (selectedPlayerType == blackPlayerDropdown.options[blackPlayerDropdown.value].text){
            // Same type selected
            whitePlayerNameInput.text = $"{selectedPlayerType}1";
            blackPlayerNameInput.text = $"{selectedPlayerType}2";
        }
        else{
            // Different types, remove any number suffix if present
            blackPlayerNameInput.text = blackPlayerNameInput.text.Replace("1", "").Replace("2", "").Trim();
        }
    }

    private void OnBlackPlayerTypeChanged(int index){
        string selectedPlayerType = blackPlayerDropdown.options[index].text;

        // Update the input field based on the selected player type
        blackPlayerNameInput.text = selectedPlayerType == "Player"? "P2" : selectedPlayerType; // Set the name to the player type

        // Check if player types are the same and update white player's name accordingly
        if (selectedPlayerType == whitePlayerDropdown.options[whitePlayerDropdown.value].text){
            // Same type selected
            blackPlayerNameInput.text = $"{selectedPlayerType}2";
            whitePlayerNameInput.text = $"{selectedPlayerType}1";
        }
        else{
            // Different types, remove any number suffix if present
            whitePlayerNameInput.text = whitePlayerNameInput.text.Replace("1", "").Replace("2", "").Trim();
        }
    }


    private TMP_Dropdown CreateDropdown(string name, Color backgroundColor)
    {
        GameObject dropdownObject = Instantiate(dropdownPrefab);
        dropdownObject.name = name;
        dropdownObject.transform.SetParent(canvas.transform, false);

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
        if (dropdown == null){
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

    private Button CreateTMPButton(string name, string buttonText){
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

    private Sprite CreateRoundedSprite(){
        // Create a rounded rectangle texture
        Texture2D texture = new Texture2D(1, 1);
        Color[] pixels = new Color[1];
        pixels[0] = Color.gray; // Set a default color
        texture.SetPixels(pixels);
        texture.Apply();

        // Create a new sprite with rounded corners
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private EventTrigger.Entry CreateEventTriggerEntry(Action action, EventTriggerType eventType){
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((data) => action());
        return entry;
    }

    private void OnMouseEnter(Button button){
        Image buttonBackground = button.GetComponent<Image>();
        buttonBackground.color = new Color(0.7f, 0.7f, 0.7f); // Change color on hover

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(1.05f, 1.05f, 1f); // Grow slightly
    }

    private void OnMouseExit(Button button){
        Image buttonBackground = button.GetComponent<Image>();
        buttonBackground.color = Color.gray; // Reset color on exit

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one; // Reset scale
    }

    private Image CreateImage(string name, Sprite sprite){
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(canvas.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;

        RectTransform rectTransform = image.GetComponent<RectTransform>();

        // Adjust these values based on your image's dimensions
        float imageWidth = sprite.rect.width;
        float imageHeight = sprite.rect.height;

        // You can adjust the size based on your design preferences
        float desiredWidth = 100; // Set desired width
        float aspectRatio = imageWidth / imageHeight;

        rectTransform.sizeDelta = new Vector2(desiredWidth, desiredWidth / aspectRatio); // Maintain aspect ratio
        rectTransform.anchorMin = new Vector2(0.5f, 1); // Anchor to the top center
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = new Vector2(0, -100); // Adjust vertical position as needed

        return image;
    }

    private TMP_InputField CreateInputField(string name, string placeholderText, Color fieldColour){
        GameObject inputFieldObject = Instantiate(inputFieldPrefab);
        inputFieldObject.name = name;
        inputFieldObject.transform.SetParent(canvas.transform, false);

        // Check for TMP_InputField component
        TMP_InputField inputField = inputFieldObject.GetComponent<TMP_InputField>();
        if (inputField == null){
            Debug.LogError("TMP_InputField component not found on instantiated prefab.");
            return null;
        }

        // Set placeholder text
        if (inputField.placeholder is TextMeshProUGUI placeholder){
            placeholder.text = placeholderText;
        }
        else{
            Debug.LogError("Placeholder not found in TMP_InputField.");
        }

        // Set background color
        Image backgroundImage = inputFieldObject.GetComponent<Image>();
        if (backgroundImage != null) {
            backgroundImage.color = fieldColour; // Set your desired background color here
        } else {
            Debug.LogError("Background image not found in TMP_InputField.");
        }

        return inputField;
    }



    private void ArrangeUI(){
        // Set the position for the image at the top center
        RectTransform logoRect = canvas.transform.Find("GameLogo").GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 1);
        logoRect.anchorMax = new Vector2(0.5f, 1);
        logoRect.anchoredPosition = new Vector2(0, -100);

        // Set the position for the input fields above the dropdowns
        RectTransform whiteInputRect = whitePlayerNameInput.GetComponent<RectTransform>();
        whiteInputRect.anchorMin = new Vector2(0.4f, 0.7f);
        whiteInputRect.anchorMax = new Vector2(0.4f, 0.7f);
        whiteInputRect.anchoredPosition = new Vector2(-60, -70);

        RectTransform blackInputRect = blackPlayerNameInput.GetComponent<RectTransform>();
        blackInputRect.anchorMin = new Vector2(0.6f, 0.7f);
        blackInputRect.anchorMax = new Vector2(0.6f, 0.7f);
        blackInputRect.anchoredPosition = new Vector2(60, -70);

        // Set the position for the dropdowns below the input fields
        RectTransform whiteDropdownRect = whitePlayerDropdown.GetComponent<RectTransform>();
        whiteDropdownRect.anchorMin = new Vector2(0.4f, 0.5f);
        whiteDropdownRect.anchorMax = new Vector2(0.4f, 0.5f);
        whiteDropdownRect.anchoredPosition = new Vector2(-60, 0);

        RectTransform blackDropdownRect = blackPlayerDropdown.GetComponent<RectTransform>();
        blackDropdownRect.anchorMin = new Vector2(0.6f, 0.5f);
        blackDropdownRect.anchorMax = new Vector2(0.6f, 0.5f);
        blackDropdownRect.anchoredPosition = new Vector2(60, 0);

        // Set the position for the button below the dropdowns
        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.anchoredPosition = new Vector2(0, -50);
    }


    private void OnStartButtonClicked(){
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

        // Get player names from input fields
        string whitePlayerName = whitePlayerNameInput.text.Trim();
        string blackPlayerName = blackPlayerNameInput.text.Trim();

        if (string.IsNullOrEmpty(whitePlayerName)) whitePlayerName = "P1"; // Default name if empty
        if (string.IsNullOrEmpty(blackPlayerName)) blackPlayerName = "P2"; // Default name if empty

        Debug.Log($"Selected White Player: {whitePlayerType} ({whitePlayerName}), Black Player: {blackPlayerType} ({blackPlayerName})");

        game.InitializeGame(whitePlayerType, blackPlayerType, whitePlayerName, blackPlayerName);
        
        Destroy(canvas.gameObject);
    }

}
