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
    private TMP_Dropdown whitePlayerDropdown, blackPlayerDropdown;
    private TMP_InputField whitePlayerNameInput, blackPlayerNameInput, filePathInput;
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
        // also for filepaths
        filePathInput = CreateInputField("FilePathInputField", "Path to Moves.txt", Color.white);

        // Create Start Button
        startButton = CreateTMPButton("StartButton", "Start Game");
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Layout
        ArrangeUI();
    }

    private void OnWhitePlayerTypeChanged(int index)=>UpdatePlayerName(whitePlayerDropdown, whitePlayerNameInput, blackPlayerDropdown, blackPlayerNameInput);
    private void OnBlackPlayerTypeChanged(int index)=>UpdatePlayerName(blackPlayerDropdown, blackPlayerNameInput, whitePlayerDropdown, whitePlayerNameInput);
    
    private void UpdatePlayerName(TMP_Dropdown currentDropdown, TMP_InputField currentInput, TMP_Dropdown otherDropdown, TMP_InputField otherInput){
        string selectedPlayerType = currentDropdown.options[currentDropdown.value].text;
        currentInput.text = selectedPlayerType == "Player" ? currentInput.name : selectedPlayerType;

        if (selectedPlayerType == otherDropdown.options[otherDropdown.value].text)
        {
            currentInput.text = $"{selectedPlayerType}1";
            otherInput.text = $"{selectedPlayerType}2";
        }
        else
        {
            otherInput.text = otherInput.text.Replace("1", "").Replace("2", "").Trim();
        }
    }


    private TMP_Dropdown CreateDropdown(string name, Color backgroundColor){
        GameObject dropdownObject = Instantiate(dropdownPrefab);
        dropdownObject.name = name;
        dropdownObject.transform.SetParent(canvas.transform, false);

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
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
        AddEventTrigger(buttonObject, button);

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

    private void AddEventTrigger(GameObject buttonObject, Button button)
    {
        var eventTrigger = buttonObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(CreateEventTriggerEntry(() => OnMouseEnter(button), EventTriggerType.PointerEnter));
        eventTrigger.triggers.Add(CreateEventTriggerEntry(() => OnMouseExit(button), EventTriggerType.PointerExit));
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

        // Set placeholder text
        if (inputField.placeholder is TextMeshProUGUI placeholder){
            placeholder.text = placeholderText;
        }

        // Set background color
        Image backgroundImage = inputFieldObject.GetComponent<Image>();
        backgroundImage.color = fieldColour; // Set your desired background color here

        return inputField;
    }


    private void ArrangeUI()
    {
        var logoRect = canvas.transform.Find("GameLogo").GetComponent<RectTransform>();
        logoRect.anchoredPosition = new Vector2(0, -100);

        SetPosition(whitePlayerNameInput, 0.4f, 0.7f, new Vector2(-60, -70));
        SetPosition(blackPlayerNameInput, 0.6f, 0.7f, new Vector2(60, -70));
        SetPosition(filePathInput, 0.5f, 0.3f, new Vector2(0, -100)); // Adjust position
        SetPosition(whitePlayerDropdown, 0.4f, 0.5f, new Vector2(-60, 0));
        SetPosition(blackPlayerDropdown, 0.6f, 0.5f, new Vector2(60, 0));
        SetPosition(startButton, 0.5f, 0.4f, new Vector2(0, -50));
    }

    private void SetPosition(Component component, float anchorX, float anchorY, Vector2 anchoredPosition)
    {
        var rectTransform = component.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(anchorX, anchorY);
        rectTransform.anchorMax = new Vector2(anchorX, anchorY);
        rectTransform.anchoredPosition = anchoredPosition;
    }
    private string ValidateFilePath(string filePath){
        if (string.IsNullOrEmpty(filePath))
            return "File path cannot be empty.";
        
        if (!System.IO.File.Exists(filePath))
            return "The specified file does not exist.";
        

        if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            return "Please provide a valid .txt file.";
        
        return null; // No errors
    }

    private void OnStartButtonClicked()
    {
        if (game == null)
        {
            Debug.LogError("Game component not found!");
            return;
        }

        string whitePlayerType = whitePlayerDropdown.options[whitePlayerDropdown.value].text;
        string blackPlayerType = blackPlayerDropdown.options[blackPlayerDropdown.value].text;

        string whitePlayerName = string.IsNullOrEmpty(whitePlayerNameInput.text.Trim()) ? "P1" : whitePlayerNameInput.text.Trim();
        string blackPlayerName = string.IsNullOrEmpty(blackPlayerNameInput.text.Trim()) ? "P2" : blackPlayerNameInput.text.Trim();

        string filePath = filePathInput.text.Trim();
        bool isPresetWhite = whitePlayerType == "Preset";
        bool isPresetBlack = blackPlayerType == "Preset";

        // Validate file path if either player is a Preset
        if (isPresetWhite || isPresetBlack)
        {
            string errorMessage = ValidateFilePath(filePath);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError(errorMessage);
                return; // Stop execution if path is invalid
            }
        }

        Debug.Log($"Selected White Player: {whitePlayerType} ({whitePlayerName}), Black Player: {blackPlayerType} ({blackPlayerName})");

        game.InitializeGame(whitePlayerType, blackPlayerType, whitePlayerName, blackPlayerName, filePath);

        // Clean up UI
        Destroy(canvas.gameObject);
    }

}