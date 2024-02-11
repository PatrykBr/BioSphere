using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // References to various settings panels
    private GameObject generalSettingsPanel;
    private GameObject soundSettingsPanel;

    // References to panels related to world management
    private GameObject worldSelectionPanel;
    private GameObject createWorldPanel;
    private GameObject editWorldPanel;

    // Variables to store user inputs for world creation/editing
    private string worldNameInput;
    private float difficultySliderValue = 1;

    // Object representing a loaded world
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
                break;
            case MenuScreen.Credits:
                creditsPanel.SetActive(true);
                break;
            case MenuScreen.PlayScreen:
                playPanel.SetActive(true);
                worldSelectionPanel.SetActive(true);
                break;
        }
    }

    // Methods to show different screens
    public void ShowMainMenu() => ShowScreen(MenuScreen.MainMenu);
    public void ShowSettings()
    {
        ShowScreen(MenuScreen.Settings);
        SelectGeneralSettings();
    }
    public void ShowCredits() => ShowScreen(MenuScreen.Credits);
    public void ShowPlayScreen()
    {
        ShowScreen(MenuScreen.PlayScreen);
        LoadAndDisplayWorlds();
    }
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
    public void ShowSliderValue(float value) => difficultySliderValue = value;

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

        // Validate world name and display appropriate error messages
        if (string.IsNullOrEmpty(newWorld.WorldName))
        {
            ShowError(errorText, "Error: World name cannot be empty.");
        }
        else if (newWorld.WorldName.Length > 15)
        {
            ShowError(errorText, "Error: World name cannot be longer than 15 characters.");
        }
        else if (newWorld.WorldName.Length < 2)
        {
            ShowError(errorText, "Error: World name cannot be less than 2 characters.");
        }
        else
        {
            // Save the new world to a JSON file
            World.WriteWorldJSON(newWorld);

            // Hide create world panel, reload and display worlds, and show world selection panel
            createWorldPanel.SetActive(false);
            LoadAndDisplayWorlds();
            worldSelectionPanel.SetActive(true);
        }
    }

    // Display error message and hide it after a specified time
    private void ShowError(TextMeshProUGUI errorText, string errorMessage)
    {
        errorText.text = errorMessage;
        errorText.gameObject.SetActive(true);
        StartCoroutine(HideErrorText(errorText));
    }

    // Edit the properties of a world template
    private void EditTemplate(World world)
    {
        string oldWorldName = world.WorldName;

        world.WorldName = worldNameInput;
        world.WorldDifficulty = (difficultySliderValue == 0) ? "Easy" : (difficultySliderValue == 1) ? "Medium" : "Hard";

        // Rename world file if necessary
        if (oldWorldName != null && world != null && oldWorldName != world.WorldName)
        {
            // Get reference to error text UI element
            TextMeshProUGUI errorText = editWorldPanel.transform.Find("EditScreen/ErrorText").GetComponent<TextMeshProUGUI>();

            Debug.Log(world.WorldName.Length);

            if (string.IsNullOrEmpty(world.WorldName))
            {
                ShowError(errorText, "Error: World name cannot be empty.");
            }
            else if (world.WorldName.Length > 15)
            {
                ShowError(errorText, "Error: World name cannot be longer than 15 characters.");
            }
            else if (world.WorldName.Length < 2)
            {
                ShowError(errorText, "Error: World name cannot be less than 2 characters.");
            }
            else
            {

                RenameWorldFile(oldWorldName, world.WorldName);

                // Write the updated world data to a JSON file
                World.WriteWorldJSON(world);

                // Show world selection panel, hide edit world panel, and reload worlds
                worldSelectionPanel.SetActive(true);
                editWorldPanel.SetActive(false);
                LoadAndDisplayWorlds();
            }
            }


    }

    // Rename the world JSON file
    private void RenameWorldFile(string oldName, string newName)
    {
        string oldFilePath = Path.Combine(World.WorldDirectory, oldName + ".json");
        string newFilePath = Path.Combine(World.WorldDirectory, newName + ".json");

            // Check if old file exists and new file doesn't exist, then rename the files
            if (!File.Exists(oldFilePath))
            {
                Debug.LogError($"Error: File {oldFilePath} not found.");
                return;
            }

            if (File.Exists(newFilePath))
            {
                Debug.LogError($"Error: File {newFilePath} already exists.");
                return;
            }

            try
            {
                // Rename the world JSON file
                File.Move(oldFilePath, newFilePath);
                Debug.Log($"Successfully renamed file from {oldName} to {newName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while renaming file: {e.Message}");
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

        loadedWorld = World.ReadWorldJSON(worldName);
        int difficultyValue = GetDifficultyValue(loadedWorld.WorldDifficulty);

        // Set UI elements for editing the world
        SetEditWorldUI(loadedWorld.WorldName, difficultyValue);

        Button saveEditButton = editWorldPanel.transform.Find("EditScreen/SaveEdit").GetComponent<Button>();
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
                return -1;
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
        foreach (Feature feature in features)
        {
            GameObject newFeature = Instantiate(FeaturetemplatePrefab, contentTransform);
            newFeature.name = feature.name;
            TextMeshProUGUI textMeshPro = newFeature.transform.Find("FeatureName").GetComponent<TextMeshProUGUI>();
            textMeshPro.text = feature.name.Replace("_", " ");
        }
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

    // Show confirmation screen for world deletion
    public void ShowConfirmationScreen()
    {
        TogglePanel("EditScreen", false);
        TogglePanel("Confirmation", true);

        SetConfirmationText($"Are you sure you would like to delete \"{loadedWorld.WorldName}\"?");
    }

    // Handle confirmation button click
    public void HandleConfirmationButtonClick(bool isDeleteConfirmed)
    {
        if (isDeleteConfirmed)
        {
            DeleteWorld();
        }
        else
        {
            TogglePanel("Confirmation", false);
            TogglePanel("EditScreen", true);
        }
    }

    // Toggle the visibility of a panel
    private void TogglePanel(string panelName, bool isActive)
    {
        GameObject panel = editWorldPanel.transform.Find(panelName).gameObject;
        panel.SetActive(isActive);
    }

    // Set text for confirmation dialog
    private void SetConfirmationText(string text)
    {
        TextMeshProUGUI confirmationText = editWorldPanel.transform.Find("Confirmation/Text").GetComponent<TextMeshProUGUI>();
        confirmationText.text = text;
    }

    // Delete the selected world
    private void DeleteWorld()
    {
        if (loadedWorld != null)
        {
            string filePathToDelete = Path.Combine(World.WorldDirectory, loadedWorld.WorldName + ".json");

            string metaFilePathToDelete = Path.Combine(World.WorldDirectory, loadedWorld.WorldName + ".json.meta");

            if (File.Exists(filePathToDelete))
            {
                File.Delete(filePathToDelete);
                Debug.Log($"World '{loadedWorld.WorldName}' deleted.");

                if (File.Exists(metaFilePathToDelete))
                {
                    File.Delete(metaFilePathToDelete);
                    Debug.Log($"Meta file '{loadedWorld.WorldName}.json.meta' deleted.");
                }

                worldSelectionPanel.SetActive(true);
                editWorldPanel.SetActive(false);
                LoadAndDisplayWorlds();
            }
            else
            {
                Debug.LogError($"Error: File '{filePathToDelete}' not found.");
            }
        }
        else
        {
            Debug.LogError("Error: loadedWorld is null.");
        }
    }

    #endregion

    // Quit the application
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
