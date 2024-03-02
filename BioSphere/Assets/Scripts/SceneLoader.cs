using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen }

public class MainMenuManager : MonoBehaviour
{
    // References to UI panels and prefabs
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;
    public GameObject creatorPanel;
    public GameObject templatePrefab;
    public GameObject FeaturetemplatePrefab;
    public GameObject FeatureModels;

    // References to various settings panels
    private GameObject generalSettingsPanel;
    private GameObject soundSettingsPanel;

    // References to panels related to world management
    private GameObject worldSelectionPanel;
    private GameObject createWorldPanel;
    private GameObject editWorldPanel;

    // Variables to store user inputs for world creation/editing
    private string worldNameInput;
    private int difficultySliderValue = 1;

    // Object representing the loaded world
    private World loadedWorld;

    // Initialize UI elements and set the main menu as the initial screen
    private void Start()
    {
        InitializeUI();
        ShowScreen(MenuScreen.MainMenu);
    }

    // Initialize references to UI elements for easy access
    private void InitializeUI()
    {
        worldSelectionPanel = playPanel.transform.Find("WorldSelection").gameObject;
        createWorldPanel = playPanel.transform.Find("CreateWorld").gameObject;
        editWorldPanel = playPanel.transform.Find("EditWorld").gameObject;

        createWorldPanel.SetActive(false);
        worldSelectionPanel.SetActive(false);
        editWorldPanel.SetActive(false);

        GameObject generalSettingsButton = settingsPanel.transform.Find("General").gameObject;
        GameObject soundSettingsButton = settingsPanel.transform.Find("Sound").gameObject;

        generalSettingsPanel = generalSettingsButton.transform.Find("Main").gameObject;
        soundSettingsPanel = soundSettingsButton.transform.Find("Main").gameObject;
    }

    #region ScreenManagement

    // Show the specified menu screen and hide all others
    private void ShowScreen(MenuScreen screen)
    {
        DisableAllPanels();
        EnableSelectedPanel(screen);
    }

    // Disable all UI panels
    private void DisableAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        playPanel.SetActive(false);
        creatorPanel.SetActive(false);
    }

    // Enable the selected panel based on the specified menu screen
    private void EnableSelectedPanel(MenuScreen screen)
    {
        switch (screen)
        {
            case MenuScreen.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case MenuScreen.Settings:
                settingsPanel.SetActive(true);
                SelectGeneralSettings();
                break;
            case MenuScreen.Credits:
                creditsPanel.SetActive(true);
                break;
            case MenuScreen.PlayScreen:
                playPanel.SetActive(true);
                worldSelectionPanel.SetActive(true);
                LoadAndDisplayWorlds();
                break;
        }
    }
    // Methods to show different screens
    public void ShowMainMenu() => ShowScreen(MenuScreen.MainMenu);
    public void ShowSettings() => ShowScreen(MenuScreen.Settings);
    public void ShowCredits() => ShowScreen(MenuScreen.Credits);
    public void ShowPlayScreen() => ShowScreen(MenuScreen.PlayScreen);
    public void GoBack()
    {
        if (IsCreateOrEditWorldPanelActive())
        {
            createWorldPanel.SetActive(false);
            editWorldPanel.SetActive(false);
            worldSelectionPanel.SetActive(true);
        }
        else if (creatorPanel.activeSelf)
        {
            creatorPanel.SetActive(false);
            playPanel.SetActive(true);
            worldSelectionPanel.SetActive(true);
        }
        else
        {
            ShowMainMenu();
        }
    }

    // Check if create or edit world panel is active
    private bool IsCreateOrEditWorldPanelActive() => createWorldPanel.activeSelf || editWorldPanel.activeSelf;

    #endregion

    #region Settings

    // Select the general settings panel
    public void SelectGeneralSettings() => ActivateSettingsPanel(generalSettingsPanel, soundSettingsPanel);

    // Select the sound settings panel
    public void SelectSoundSettings() => ActivateSettingsPanel(soundSettingsPanel, generalSettingsPanel);

    // Activate the specified settings panel and deactivate the other
    private void ActivateSettingsPanel(GameObject activePanel, GameObject inactivePanel)
    {
        activePanel.SetActive(true);
        inactivePanel.SetActive(false);
    }

    #endregion

    #region PlayScreen

    // Reset the create world panel UI elements
    private void ResetCreateWorldPanel()
    {
        TMP_InputField worldNameInputField = createWorldPanel.transform.Find("WorldName").GetComponent<TMP_InputField>();
        worldNameInputField.text = "";

        Slider difficultySlider = createWorldPanel.transform.Find("Difficulty/Slider").GetComponent<Slider>();
        difficultySlider.value = 1;
    }

    // Handle create button click event
    public void OnCreateButtonPressed()
    {
        ResetCreateWorldPanel();

        worldSelectionPanel.SetActive(false);
        createWorldPanel.SetActive(true);
    }

    // Read user input for world name
    public void ReadStringInput(string input) => worldNameInput = input;

    // Display the value of the difficulty slider
    public void ShowSliderValue(int value) => difficultySliderValue = value;

    // Validates a proposed world name, ensuring it meets specific criteria.
    private bool ValidateWorldName(TextMeshProUGUI errorText, string newName)
    {
        var worldPath = Path.Combine(World.WorldDirectory, newName + ".json");

        // Check if world name already exits
        if (File.Exists(worldPath))
        {
            ShowError(errorText, "Error: World name already exists.");
            return false;
        }

        // Check for empty name:
        if (string.IsNullOrEmpty(newName))
        {
            ShowError(errorText, "Error: World name cannot be empty.");
            return false;
        }

        // Enforce length restrictions (2-15 characters):
        else if (newName.Length > 15)
        {
            ShowError(errorText, "Error: World name cannot be longer than 15 characters.");
            return false;
        }
        else if (newName.Length < 2)
        {
            ShowError(errorText, "Error: World name cannot be less than 2 characters.");
            return false;
        }

        // Name meets criteria:
        return true;
    }

    // Coroutine to hide error text after a specified time
    private IEnumerator HideErrorText(TextMeshProUGUI errorText)
    {
        yield return new WaitForSeconds(3);
        errorText.gameObject.SetActive(false);
    }

    // Create a new world based on user input
    public void CreateWorld()
    {
        // Create a new world object
        World newWorld = new World(
            worldNameInput,
            (difficultySliderValue == 0) ? "Easy" : (difficultySliderValue == 1) ? "Medium" : "Hard"
        )
        {
            Features = new string[] { "Small_Fins", "Small_Body", "Small_Eyes" }
        };

        // Get reference to error text UI element
        TextMeshProUGUI errorText = createWorldPanel.transform.Find("ErrorText").GetComponent<TextMeshProUGUI>();

        // Validate the new name:
        if (!ValidateWorldName(errorText, newWorld.WorldName))
        {
            return; // Exit if validation fails
        }

        // Save the new world to a JSON file
        World.WriteWorldJSON(newWorld);

        // Hide create world panel, reload and display worlds, and show world selection panel
        createWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
        worldSelectionPanel.SetActive(true);

    }

    // Display error message and hide it after a specified time
    private void ShowError(TextMeshProUGUI errorText, string errorMessage)
    {
        errorText.text = errorMessage;
        errorText.gameObject.SetActive(true);
        StartCoroutine(HideErrorText(errorText));
    }

    // Returns the text values from the difficulty
    private string GetDifficultyString(int difficultyValue)
    {
        switch (difficultyValue)
        {
            case 0:
                return "Easy";
            case 1:
                return "Medium";
            case 2:
                return "Hard";
            default:
                return "Medium"; // Default fallback
        }
    }

    // Handles editing a World object based on user input.
    private void EditTemplate(World world)
    {
        string oldWorldName = world.WorldName;  // Store original name for potential file renaming
        TextMeshProUGUI errorText = editWorldPanel.transform.Find("EditScreen/ErrorText").GetComponent<TextMeshProUGUI>();

        // Update world name if input differs from original:
        if (worldNameInput != null && worldNameInput != oldWorldName)
        {
            if (!ValidateWorldName(errorText, worldNameInput))
            {
                return; // Exit if validation fails
            }
            world.WorldName = worldNameInput;

            // Rename the world file on disk to reflect the new name:
            RenameWorldFile(oldWorldName, worldNameInput);
        }

        // Update world difficulty based on slider value:
        world.WorldDifficulty = GetDifficultyString(difficultySliderValue);

        // Persist world changes to JSON file:
        World.WriteWorldJSON(world);

        // Navigate to world selection screen:
        worldSelectionPanel.SetActive(true);
        editWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
    }


    // Renames the world JSON file, ensuring proper error handling and logging.
    private void RenameWorldFile(string oldName, string newName)
    {
        try
        {
            // Construct file paths using Path.Combine for consistency and clarity.
            var oldPath = Path.Combine(World.WorldDirectory, oldName + ".json");
            var newPath = Path.Combine(World.WorldDirectory, newName + ".json");

            // Validate file existence:
            if (!File.Exists(oldPath))
            {
                throw new FileNotFoundException($"File '{oldPath}' not found.");
            }

            // Prevent accidental overwrites:
            if (File.Exists(newPath))
            {
                throw new IOException($"File '{newPath}' already exists.");
            }

            // Perform the renaming operation:
            File.Move(oldPath, newPath);

            // Log success for tracking and debugging:
            Debug.Log($"Successfully renamed file from '{oldName}' to '{newName}'.");
        }
        catch (Exception e)
        {
            // Log errors for troubleshooting:
            Debug.LogError($"Error renaming world file: {e.Message}");
        }
    }

    // Load and display available worlds
    private void LoadAndDisplayWorlds()
    {
        Transform contentTransform = worldSelectionPanel.transform.Find("Scroll View/Viewport/Content");

        // Clear existing world entries
        ClearWorldEntries(contentTransform);

        // Get all JSON files in the world directory
        string[] jsonFiles = Directory.GetFiles(World.WorldDirectory, "*.json");

        // Instantiate UI entries for each world
        foreach (string jsonFile in jsonFiles)
        {
            string worldName = Path.GetFileNameWithoutExtension(jsonFile);
            InstantiateWorldEntry(contentTransform, worldName);
        }
    }

    // Clear existing world entries from the UI
    private void ClearWorldEntries(Transform contentTransform)
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    // Instantiate UI entry for a world
    private void InstantiateWorldEntry(Transform parentTransform, string worldName)
    {
        GameObject templateInstance = Instantiate(templatePrefab, parentTransform);
        templateInstance.SetActive(true);

        TextMeshProUGUI nameText = templateInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        nameText.text = worldName;

        Button playButton = templateInstance.transform.Find("PlayButton").GetComponent<Button>();
        playButton.onClick.AddListener(() => LoadWorld(worldName));

        Button settingsButton = templateInstance.transform.Find("SettingsButton").GetComponent<Button>();
        settingsButton.onClick.AddListener(() => LoadSettings(worldName));
    }

    // Load settings of a world for editing
    private void LoadSettings(string worldName)
    {
        GameObject confirmationScreen = editWorldPanel.transform.Find("Confirmation").gameObject;
        Button saveEditButton = editWorldPanel.transform.Find("EditScreen/SaveEdit").GetComponent<Button>();
        loadedWorld = World.ReadWorldJSON(worldName);
        int difficultyValue = GetDifficultyValue(loadedWorld.WorldDifficulty);

        // Set UI elements for editing the world
        SetEditWorldUI(loadedWorld.WorldName, difficultyValue);

        saveEditButton.onClick.RemoveAllListeners();
        saveEditButton.onClick.AddListener(() => EditTemplate(loadedWorld));

        worldSelectionPanel.SetActive(false);
        confirmationScreen.SetActive(false);
        editWorldPanel.SetActive(true);
        editWorldPanel.transform.Find("EditScreen").gameObject.SetActive(true);
    }

    // Set UI elements for editing the world
    private void SetEditWorldUI(string worldName, int difficultyValue)
    {
        editWorldPanel.transform.Find("EditScreen/WorldName").GetComponent<TMP_InputField>().text = worldName;
        editWorldPanel.transform.Find("EditScreen/Difficulty/Slider").GetComponent<Slider>().value = difficultyValue;
    }

    // Get the slider value corresponding to the difficulty level
    private int GetDifficultyValue(string difficulty)
    {
        switch (difficulty)
        {
            case "Easy":
                return 0;
            case "Medium":
                return 1;
            case "Hard":
                return 2;
            default:
                return 1;
        }
    }

    // Show confirmation screen for world deletion
    public void ShowConfirmationScreen()
    {
        GameObject EditScreen = editWorldPanel.transform.Find("EditScreen").gameObject;
        GameObject ConfirmationScreen = editWorldPanel.transform.Find("Confirmation").gameObject;
        EditScreen.SetActive(false);
        ConfirmationScreen.SetActive(true);

        TextMeshProUGUI confirmationText = ConfirmationScreen.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        confirmationText.text = $"Are you sure you would like to delete \"{loadedWorld.WorldName}\"?";
    }

    // Deletes the selected world file
    public void DeleteWorld()
    {
        // Ensure a world is loaded before attempting deletion.
        if (loadedWorld == null)
        {
            Debug.LogError("Error: Cannot delete world, loadedWorld is null.");
            return;
        }

        string filePathToDelete = Path.Combine(World.WorldDirectory, loadedWorld.WorldName + ".json");

        // Verify that the file exists before attempting deletion.
        if (!File.Exists(filePathToDelete))
        {
            Debug.LogError($"Error: File '{filePathToDelete}' not found.");
            return;
        }

        try
        {
            File.Delete(filePathToDelete);
            Debug.Log($"World '{loadedWorld.WorldName}' deleted.");

            worldSelectionPanel.SetActive(true);
            editWorldPanel.SetActive(false);
            LoadAndDisplayWorlds();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting world file: {ex.Message}");
        }
    }

    // Load a selected world for editing
    private void LoadWorld(string worldName)
    {
        World loadedWorld = World.ReadWorldJSON(worldName);
        ClearWorldEntries(creatorPanel.transform.Find("Scroll View/Viewport/Content"));

        List<Feature> features = FeatureFinder.GetFeatures(loadedWorld);
        CreateFeatures(features);

        CalculateAndDisplayStats(loadedWorld);

        creatorPanel.SetActive(true);
        playPanel.SetActive(false);
    }

    // Create UI elements for world features
    private void CreateFeatures(List<Feature> features)
    {
        Transform contentTransform = creatorPanel.transform.Find("Scroll View/Viewport/Content");

        // Load all GameObjects from the "FeatureModels" folder
        GameObject[] featureObjects = Resources.LoadAll<GameObject>("FeatureModels");

        foreach (Feature feature in features)
        {
            GameObject newFeature = Instantiate(FeaturetemplatePrefab, contentTransform);
            newFeature.name = feature.name;
            TextMeshProUGUI textMeshPro = newFeature.transform.Find("FeatureName").GetComponent<TextMeshProUGUI>();
            textMeshPro.text = feature.name.Replace("_", " ");

            foreach (GameObject obj in featureObjects)
            {
                if (obj.name == feature.name)
                {
                    RawImage rawImage = newFeature.transform.Find("FeatureIcon").GetComponent<RawImage>();

                    // Render the 3D object to a texture
                    Texture2D renderTexture = Render3DObjectToTexture(obj);

                    // Display the rendered texture in the UI RawImage
                    if (renderTexture != null)
                    {
                        rawImage.texture = renderTexture;
                    }

                    break; // Stop searching once the object is found
                }
            }
        }
    }

    // Function to render 3D object to texture
    private Texture2D Render3DObjectToTexture(GameObject objPrefab)
    {
        // Create a RenderTexture with the desired dimensions
        RenderTexture renderTexture = new(256, 256, 24);
        int LayerIndex = LayerMask.NameToLayer("UiItems");

        // Create a camera for rendering the object
        GameObject cameraObject = new("RenderCamera");
        Camera renderCamera = cameraObject.AddComponent<Camera>();
        renderCamera.targetTexture = renderTexture;
        renderCamera.enabled = false;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        //renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        renderCamera.cullingMask = 1 << LayerIndex;

        // Instantiate the object in the scene
        GameObject obj = Instantiate(objPrefab);
        obj.transform.position = renderCamera.transform.position + renderCamera.transform.forward * 2f;
        obj.transform.LookAt(renderCamera.transform);
        obj.layer = LayerIndex;

        // Render the object
        renderCamera.Render();

        // Create a new Texture2D to hold the rendered image
        Texture2D texture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // Read the pixels from the render texture and apply them to the texture
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Release the render texture and cleanup
        RenderTexture.active = null;
        Destroy(cameraObject);
        Destroy(obj);

        return texture;
    }


private void ApplyRenderTextureToRawImage(RenderTexture renderTexture, RawImage rawImage)
    {
        // Create a new texture to hold the rendered image
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // Read the pixels from the render texture and apply them to the texture
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Apply the texture to the RawImage component
        rawImage.texture = texture;
    }



    // Calculate and display statistics based on selected features
    private void CalculateAndDisplayStats(World world)
    {
        int totalHealth = 0;
        int totalSpeed = 0;
        int totalStrength = 0;

        foreach (string selectedFeature in world.SelectedFeatures)
        {
            Feature feature = FeatureFinder.FindFeatureInItems(selectedFeature);
            if (feature != null)
            {
                totalHealth += feature.stat.health;
                totalSpeed += feature.stat.speed;
                totalStrength += feature.stat.strength;
            }
            else
            {
                Debug.Log("Could not find feature with name " + selectedFeature);
                continue;
            }
        }

        UpdateStatsUI(totalHealth, totalSpeed, totalStrength);
    }

    // Update UI with calculated statistics
    private void UpdateStatsUI(int health, int speed, int strength)
    {
        Transform statsFrame = creatorPanel.transform.Find("Stats");
        TextMeshProUGUI healthTxt = statsFrame.transform.Find("Health/TxtVal").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI speedTxt = statsFrame.transform.Find("Speed/TxtVal").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI strengthTxt = statsFrame.transform.Find("Strength/TxtVal").GetComponent<TextMeshProUGUI>();

        healthTxt.text = health.ToString();
        speedTxt.text = speed.ToString();
        strengthTxt.text = strength.ToString();

        UpdateTextColor(healthTxt, health);
        UpdateTextColor(speedTxt, speed);
        UpdateTextColor(strengthTxt, strength);
    }

    // Update text color based on the value
    private void UpdateTextColor(TextMeshProUGUI text, int value)
    {
        if (value >= 0 && value <= 5)
        {
            text.color = Color.red;
        }
        else if (value >= 6 && value <= 15)
        {
            text.color = new Color(1, 0.5f, 0);
        }
        else if (value >= 16)
        {
            text.color = Color.green;
        }
    }

    #endregion

    // Quit the game
    public void QuitGame()
    {
        //Application.Quit();
    }
}
