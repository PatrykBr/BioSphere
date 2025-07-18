using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen, Creator, Pause, Gameplay }

public class MainMenuManager : MonoBehaviour
{
    // References to UI panels and prefabs
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;
    public GameObject pausePanel;
    public GameObject gameplayPanel;
    public GameObject creatorPanel;
    public GameObject templatePrefab;
    public GameObject FeatureTemplatePrefab;

    // References to various settings panels
    private GameObject generalSettingsPanel;
    private GameObject soundSettingsPanel;

    // References to panels related to world management
    private GameObject worldSelectionPanel;
    private GameObject createWorldPanel;
    private GameObject editWorldPanel;

    //Get the gameplay script
    private Gameplay gameplay;

    // Variables to store user inputs for world creation/editing
    private string worldNameInput;
    private int difficultySliderValue = 1;

    // Object representing the loaded world
    private World loadedWorld;

    // Initialize UI elements and set the main menu as the initial screen
    private void Start()
    {
        gameplay = GetComponent<Gameplay>();
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

    // Show the specified menu screen and hide all others
    public void ShowScreen(MenuScreen screen)
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
        pausePanel.SetActive(false);
        gameplayPanel.SetActive(false);
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
            case MenuScreen.Creator:
                creatorPanel.SetActive(true);
                break;
            case MenuScreen.Pause:
                pausePanel.SetActive(true);
                break;
            case MenuScreen.Gameplay:
                gameplayPanel.SetActive(true);
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
        World newWorld = new(
            worldNameInput,
            (difficultySliderValue == 0) ? "Easy" : (difficultySliderValue == 1) ? "Medium" : "Hard"
        )
        {
            AvailableFeatures = new string[] {
                "Red_Fins",
        "Blue_Fins",
        "Green_Fins",
        "Red_Body",
        "Blue_Body",
        "Green_Body",
        "Red_Eyes",
        "Blue_Eyes",
        "Green_Eyes" }
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
        DestroyChildren(contentTransform);

        // Get all JSON files in the world directory
        string[] jsonFiles = Directory.GetFiles(World.WorldDirectory, "*.json");

        // Instantiate UI entries for each world
        foreach (string jsonFile in jsonFiles)
        {
            string worldName = Path.GetFileNameWithoutExtension(jsonFile);
            InstantiateWorldEntry(contentTransform, worldName);
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
        playButton.onClick.AddListener(() => LoadWorld("Body", worldName));

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

    public void CancelDelete()
    {
        GameObject EditScreen = editWorldPanel.transform.Find("EditScreen").gameObject;
        GameObject ConfirmationScreen = editWorldPanel.transform.Find("Confirmation").gameObject;
        EditScreen.SetActive(true);
        ConfirmationScreen.SetActive(false);
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

    // Load the world based on the provided category and world name
    private void LoadWorld(string category, string worldName)
    {
        if (!string.IsNullOrEmpty(worldName))
        {
            loadedWorld = World.ReadWorldJSON(worldName);
        }

        // Clear existing UI elements
        DestroyChildren(creatorPanel.transform.Find("Scroll View/Viewport/Content"));

        // Filter features based on the provided category
        List<CreatureFeature> filteredFeatures = CreatureManager.GetFeaturesFromWorld(loadedWorld, "AvailableFeatures")
            .Where(feature => feature.name.Contains(category)).ToList();

        List<CreatureFeature> selectedFeatures = CreatureManager.GetFeaturesFromWorld(loadedWorld, "SelectedFeatures");

        // Create UI elements for filtered features and selected features
        CreateFeaturesUI(filteredFeatures);
        CreateSelectedFeaturesUI(selectedFeatures);

        (int totalHealth, int totalSpeed, int totalStrength) = CreatureManager.CalculateStats(loadedWorld);
        UpdateStatsUI(totalHealth, totalSpeed, totalStrength);

        ShowScreen(MenuScreen.Creator);
    }

    // Create UI elements for selected features
    private void CreateSelectedFeaturesUI(List<CreatureFeature> features)
    {
        Transform contentTransform = creatorPanel.transform.Find("Creature");

        DestroyChildren(contentTransform);

        (Camera renderCamera, RenderTexture renderTexture) = CreateRenderComponents();

        GameObject body = CreatureManager.CreateSelectedFeatureModel(features);
        body.transform.SetParent(contentTransform);


        // Render the texture of the instantiated body feature
        RenderTextureToRawImage(body, creatorPanel.transform.Find("Creature").GetComponent<RawImage>(), renderCamera, renderTexture);

        // Clean up render components
        Destroy(renderCamera.gameObject);
        Destroy(renderTexture);
    }

    // Create UI elements for world features
    private void CreateFeaturesUI(List<CreatureFeature> features)
    {
        Transform contentTransform = creatorPanel.transform.Find("Scroll View/Viewport/Content");

        // Load all feature models
        GameObject[] featureObjects = Resources.LoadAll<GameObject>("FeaturePrefabs");

        (Camera renderCamera, RenderTexture renderTexture) = CreateRenderComponents();

        foreach (CreatureFeature feature in features)
        {
            GameObject newFeatureUI = Instantiate(FeatureTemplatePrefab, contentTransform);
            newFeatureUI.name = feature.name;

            TextMeshProUGUI textMeshPro = newFeatureUI.transform.Find("FeatureName").GetComponent<TextMeshProUGUI>();
            textMeshPro.text = feature.name.Replace("_", " ");

            // Find and render the texture of the feature icon
            GameObject featureObject = featureObjects.FirstOrDefault(obj => obj.name == feature.name);
            if (featureObject != null)
                RenderTextureToRawImage(featureObject, newFeatureUI.transform.Find("FeatureIcon").GetComponent<RawImage>(), renderCamera, renderTexture);

            Button featureButton = newFeatureUI.GetComponent<Button>();
            featureButton.onClick.AddListener(() => SelectFeature(feature.name));
        }

        Destroy(renderCamera.gameObject);
        Destroy(renderTexture);
    }

    private void SelectFeature(string featureName)
    {
        List<string> updatedFeatures = new(loadedWorld.SelectedFeatures);

        // Check if the feature name contains any of the keywords "Body", "Eyes", or "Fins"
        string keyword = GetKeywordFromFeatureName(featureName);

        if (!string.IsNullOrEmpty(keyword))
        {
            // If the feature is being selected, add it to the list

            updatedFeatures.RemoveAll(f => f.Contains(keyword));
            updatedFeatures.Add(featureName);

        }

        // Update the SelectedFeatures array in the loadedWorld object
        loadedWorld.SelectedFeatures = updatedFeatures.ToArray();

        // Save the new world to a JSON file
        World.WriteWorldJSON(loadedWorld);

        List<CreatureFeature> selectedFeatures = CreatureManager.GetFeaturesFromWorld(loadedWorld, "SelectedFeatures");

        CreateSelectedFeaturesUI(selectedFeatures);

        (int totalHealth, int totalSpeed, int totalStrength) = CreatureManager.CalculateStats(loadedWorld);
        UpdateStatsUI(totalHealth, totalSpeed, totalStrength);
    }

    private string GetKeywordFromFeatureName(string featureName)
    {
        if (featureName.Contains("Body"))
            return "Body";
        else if (featureName.Contains("Eye"))
            return "Eye";
        else if (featureName.Contains("Fin"))
            return "Fin";
        else
            return null;
    }

    // Render the texture of an object to a RawImage
    private void RenderTextureToRawImage(GameObject objPrefab, RawImage rawImage, Camera renderCamera, RenderTexture renderTexture)
    {
        // Instantiate the object to render
        GameObject instantiatedObject = Instantiate(objPrefab);
        Bounds bounds = CalculateObjectBounds(instantiatedObject);

        // Calculate camera distance and position
        float cameraDistance = 1.0f;
        float objectSize = CalculateObjectSize(bounds);
        float distance = CalculateCameraDistance(renderCamera, cameraDistance, objectSize);
        renderCamera.transform.position = bounds.center - distance * renderCamera.transform.forward;

        // Set object and its children to the UI layer
        instantiatedObject.layer = LayerMask.NameToLayer("UiItems");
        SetLayerRecursively(instantiatedObject.transform, LayerMask.NameToLayer("UiItems"));

        // Render the camera view and apply the texture to the RawImage
        renderCamera.Render();
        Texture2D texture = CaptureRenderTexture(renderTexture);
        DestroyImmediate(instantiatedObject);
        rawImage.texture = texture;
    }

    // Set the layer of an object and its children recursively
    private void SetLayerRecursively(Transform objTransform, int layer)
    {
        objTransform.gameObject.layer = layer;
        foreach (Transform childTransform in objTransform)
            SetLayerRecursively(childTransform, layer);
    }

    // Calculate the bounds of an object
    private Bounds CalculateObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = new(obj.transform.position, Vector3.zero);
        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    // Calculate the size of an object
    private float CalculateObjectSize(Bounds bounds)
    {
        Vector3 objectSizes = bounds.max - bounds.min;
        return Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
    }

    // Calculate the distance of the camera from the object
    private float CalculateCameraDistance(Camera renderCamera, float cameraDistance, float objectSize)
    {
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * renderCamera.fieldOfView);
        float distance = cameraDistance * objectSize / cameraView;
        distance += 0.5f * objectSize;
        return distance;
    }

    // Capture the render texture and return as a Texture2D
    private Texture2D CaptureRenderTexture(RenderTexture renderTexture)
    {
        Texture2D texture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        return texture;
    }

    // Create camera and render texture components
    private (Camera, RenderTexture) CreateRenderComponents()
    {
        RenderTexture renderTexture = new(256, 256, 24);
        GameObject cameraObject = new("RenderCamera");
        Camera renderCamera = cameraObject.AddComponent<Camera>();

        renderCamera.targetTexture = renderTexture;
        renderCamera.enabled = false;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        renderCamera.cullingMask = 1 << LayerMask.NameToLayer("UiItems");

        return (renderCamera, renderTexture);
    }

    // Clear all children of a transform
    private void DestroyChildren(Transform parentTransform)
    {
        foreach (Transform childTransform in parentTransform)
            Destroy(childTransform.gameObject);
    }

    public void ShowFeatureTab(string category)
    {
        LoadWorld(category, null);
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

    public void PlayButtonPress()
    {
        gameplay.InitGame(loadedWorld);
        creatorPanel.SetActive(false);

    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
