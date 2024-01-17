using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen }

public class MainMenuManager : MonoBehaviour
{
    // UI Panels
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;
    public GameObject creatorPanel;
    public GameObject templatePrefab;
    public GameObject FeaturetemplatePrefab;

    // Settings UI
    private GameObject generalSettingsPanel;
    private GameObject soundSettingsPanel;

    // Play Screen UI
    private GameObject worldSelectionPanel;
    private GameObject createWorldPanel;
    private GameObject editWorldPanel;

    // Input variables
    private string worldNameInput;
    private float difficultySliderValue = 1;

    private World loadedWorld;

    private void Start()
    {
        // Initialize UI components and show the main menu at the start
        InitializeUI();
        ShowScreen(MenuScreen.MainMenu);
    }

    private void InitializeUI()
    {
        // Retrieve references to various UI panels and set initial states
        worldSelectionPanel = playPanel.transform.Find("WorldSelection").gameObject;
        createWorldPanel = playPanel.transform.Find("CreateWorld").gameObject;
        editWorldPanel = playPanel.transform.Find("EditWorld").gameObject;

        createWorldPanel.SetActive(false);
        worldSelectionPanel.SetActive(false);
        editWorldPanel.SetActive(false);

        // Set up references for settings buttons and panels
        GameObject generalSettingsButton = settingsPanel.transform.Find("General").gameObject;
        GameObject soundSettingsButton = settingsPanel.transform.Find("Sound").gameObject;

        generalSettingsPanel = generalSettingsButton.transform.Find("Main").gameObject;
        soundSettingsPanel = soundSettingsButton.transform.Find("Main").gameObject;
    }

    #region ScreenManagement

    private void ShowScreen(MenuScreen screen)
    {
        DisableAllPanels();
        EnableSelectedPanel(screen);
    }

    private void DisableAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        playPanel.SetActive(false);
        creatorPanel.SetActive(false);
    }

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

    public void ShowMainMenu() => ShowScreen(MenuScreen.MainMenu);

    public void ShowSettings()
    {
        // Show the settings screen and select the general settings by default
        ShowScreen(MenuScreen.Settings);
        SelectGeneralSettings();
    }

    public void ShowCredits() => ShowScreen(MenuScreen.Credits);

    public void ShowPlayScreen()
    {
        // Show the play screen and load/display available worlds
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

    private bool IsCreateOrEditWorldPanelActive() => createWorldPanel.activeSelf || editWorldPanel.activeSelf;

    #endregion

    #region Settings

    public void SelectGeneralSettings() => ActivateSettingsPanel(generalSettingsPanel, soundSettingsPanel);

    public void SelectSoundSettings() => ActivateSettingsPanel(soundSettingsPanel, generalSettingsPanel);

    private void ActivateSettingsPanel(GameObject activePanel, GameObject inactivePanel)
    {
        // Activate the selected settings panel and deactivate the other
        activePanel.SetActive(true);
        inactivePanel.SetActive(false);
    }

    #endregion

    #region PlayScreen

    private void ResetCreateWorldPanel()
    {
        TMP_InputField worldNameInputField = createWorldPanel.transform.Find("WorldName").GetComponent<TMP_InputField>();
        worldNameInputField.text = "";

        Slider difficultySlider = createWorldPanel.transform.Find("Difficulty/Slider").GetComponent<Slider>();
        difficultySlider.value = 1;
    }

    public void OnCreateButtonPressed()
    {
        ResetCreateWorldPanel();

        worldSelectionPanel.SetActive(false);
        createWorldPanel.SetActive(true);
    }

    public void ReadStringInput(string input) => worldNameInput = input;

    public void ShowSliderValue(float value) => difficultySliderValue = value;

    private IEnumerator HideErrorText(TextMeshProUGUI errorText)
    {
        yield return new WaitForSeconds(3);
        errorText.gameObject.SetActive(false);
    }

    public void CreateWorld()
    {
        World newWorld = new World(
            worldNameInput,
            (difficultySliderValue == 0) ? "Easy" : (difficultySliderValue == 1) ? "Medium" : "Hard"
        )
        {
            Features = new string[] { "Small_Fins", "Small_Body", "Small_Eyes" }
        };

        TextMeshProUGUI errorText = createWorldPanel.transform.Find("ErrorText").GetComponent<TextMeshProUGUI>();

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
            World.WriteWorldJSON(newWorld);

            createWorldPanel.SetActive(false);
            LoadAndDisplayWorlds();
            worldSelectionPanel.SetActive(true);
        }
    }

    private void ShowError(TextMeshProUGUI errorText, string errorMessage)
    {
        errorText.text = errorMessage;
        errorText.gameObject.SetActive(true);
        StartCoroutine(HideErrorText(errorText));
    }

    private void EditTemplate(World world)
    {
        string oldWorldName = world.WorldName;

        world.WorldName = worldNameInput;
        world.WorldDifficulty = (difficultySliderValue == 0) ? "Easy" : (difficultySliderValue == 1) ? "Medium" : "Hard";

        if (oldWorldName != null && world != null && oldWorldName != world.WorldName)
        {
            RenameWorldFile(oldWorldName, world.WorldName);
        }

        World.WriteWorldJSON(world);

        worldSelectionPanel.SetActive(true);
        editWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
    }

    private void RenameWorldFile(string oldName, string newName)
    {
        string oldFilePath = Path.Combine(World.WorldDirectory, oldName + ".json");
        string newFilePath = Path.Combine(World.WorldDirectory, newName + ".json");
        string oldMetaFilePath = Path.Combine(World.WorldDirectory, oldName + ".json.meta");
        string newMetaFilePath = Path.Combine(World.WorldDirectory, newName + ".json.meta");

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
            File.Move(oldFilePath, newFilePath);
            Debug.Log($"Successfully renamed file from {oldName} to {newName}");

            if (File.Exists(oldMetaFilePath))
            {
                File.Move(oldMetaFilePath, newMetaFilePath);
                Debug.Log($"Successfully renamed meta file from {oldName}.json.meta to {newName}.json.meta");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while renaming file: {e.Message}");
        }
    }

    private void LoadAndDisplayWorlds()
    {
        Transform contentTransform = worldSelectionPanel.transform.Find("Scroll View/Viewport/Content");

        ClearWorldEntries(contentTransform);

        string[] jsonFiles = Directory.GetFiles(World.WorldDirectory, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            string worldName = Path.GetFileNameWithoutExtension(jsonFile);
            InstantiateWorldEntry(contentTransform, worldName);
        }
    }

    private void ClearWorldEntries(Transform contentTransform)
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

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

    private void LoadSettings(string worldName)
    {
        GameObject confirmationScreen = editWorldPanel.transform.Find("Confirmation").gameObject;

        loadedWorld = World.ReadWorldJSON(worldName);
        int difficultyValue = GetDifficultyValue(loadedWorld.WorldDifficulty);

        SetEditWorldUI(loadedWorld.WorldName, difficultyValue);

        Button saveEditButton = editWorldPanel.transform.Find("EditScreen/SaveEdit").GetComponent<Button>();
        saveEditButton.onClick.RemoveAllListeners();
        saveEditButton.onClick.AddListener(() => EditTemplate(loadedWorld));

        worldSelectionPanel.SetActive(false);
        confirmationScreen.SetActive(false);
        editWorldPanel.SetActive(true);
        editWorldPanel.transform.Find("EditScreen").gameObject.SetActive(true);
    }

    private void SetEditWorldUI(string worldName, int difficultyValue)
    {
        editWorldPanel.transform.Find("EditScreen/WorldName").GetComponent<TMP_InputField>().text = worldName;
        editWorldPanel.transform.Find("EditScreen/Difficulty/Slider").GetComponent<Slider>().value = difficultyValue;
    }

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



    public void ShowConfirmationScreen()
    {
        TogglePanel("EditScreen", false);
        TogglePanel("Confirmation", true);

        SetConfirmationText($"Are you sure you would like to delete \"{loadedWorld.WorldName}\"?");
    }

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

    private void TogglePanel(string panelName, bool isActive)
    {
        GameObject panel = editWorldPanel.transform.Find(panelName).gameObject;
        panel.SetActive(isActive);
    }

    private void SetConfirmationText(string text)
    {
        TextMeshProUGUI confirmationText = editWorldPanel.transform.Find("Confirmation/Text").GetComponent<TextMeshProUGUI>();
        confirmationText.text = text;
    }

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

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
