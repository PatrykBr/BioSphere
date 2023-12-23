using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // Settings UI
    private GameObject GeneralMainFrame;
    private GameObject SoundMainFrame;

    // Play Screen UI
    private GameObject WorldSelectionPanel;
    private GameObject CreateWorldPanel;
    private GameObject EditWorldPanel;

    // Input variables
    private string input;
    private float slider = 1;

    private World loadedWorld;

    private MenuScreen currentScreen;

    private void Start()
    {
        InitializeUI();
        ShowScreen(MenuScreen.MainMenu);
    }

    private void InitializeUI()
    {
        WorldSelectionPanel = playPanel.transform.Find("WorldSelection").gameObject;
        CreateWorldPanel = playPanel.transform.Find("CreateWorld").gameObject;
        EditWorldPanel = playPanel.transform.Find("EditWorld").gameObject;

        CreateWorldPanel.SetActive(false);
        WorldSelectionPanel.SetActive(false);
        EditWorldPanel.SetActive(false);

        GameObject generalButton = settingsPanel.transform.Find("General").gameObject;
        GameObject soundButton = settingsPanel.transform.Find("Sound").gameObject;

        GeneralMainFrame = generalButton.transform.Find("Main").gameObject;
        SoundMainFrame = soundButton.transform.Find("Main").gameObject;
    }

    #region ScreenManagement

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
        if (CreateOrEditWorldPanelActive())
        {
            CreateWorldPanel.SetActive(false);
            EditWorldPanel.SetActive(false);
            WorldSelectionPanel.SetActive(true);
        }
        else
        {
            ShowMainMenu();
        }
    }

    private bool CreateOrEditWorldPanelActive() => CreateWorldPanel.activeSelf || EditWorldPanel.activeSelf;

    private void ShowScreen(MenuScreen screen)
    {
        currentScreen = screen;
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
                WorldSelectionPanel.SetActive(true);
                break;
        }
    }

    #endregion

    #region Settings

    public void SelectGeneralSettings() => ActivateSettingsPanel(GeneralMainFrame, SoundMainFrame);

    public void SelectSoundSettings() => ActivateSettingsPanel(SoundMainFrame, GeneralMainFrame);

    private void ActivateSettingsPanel(GameObject activePanel, GameObject inactivePanel)
    {
        activePanel.SetActive(true);
        inactivePanel.SetActive(false);
    }

    #endregion

    #region PlayScreen

    public void OnCreateButtonPressed()
    {
        WorldSelectionPanel.SetActive(false);
        CreateWorldPanel.SetActive(true);
    }

    public void ReadStringInput(string s) => input = s;

    public void ShowSliderValue(float f) => slider = f;

    public void CreateWorld()
    {
        World writeWorld = new World(
            input,
            (slider == 0) ? "Easy" : (slider == 1) ? "Medium" : "Hard"
        )
        {
            Features = new string[] { "Small_Fin" }
        };

        if (!string.IsNullOrEmpty(writeWorld.WorldName))
        {
            World.WriteWorldJSON(writeWorld);
        }

        CreateWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
        WorldSelectionPanel.SetActive(true);
    }

    private void EditTemplate(World loadedWorld)
    {
        string oldWorldName = loadedWorld.WorldName;

        loadedWorld.WorldName = input;
        loadedWorld.WorldDifficulty = (slider == 0) ? "Easy" : (slider == 1) ? "Medium" : "Hard";

        if (oldWorldName != null && loadedWorld != null && oldWorldName != loadedWorld.WorldName)
        {
            RenameWorldFile(oldWorldName, loadedWorld.WorldName);
        }

        World.WriteWorldJSON(loadedWorld);

        WorldSelectionPanel.SetActive(true);
        EditWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
    }

    private void RenameWorldFile(string oldName, string newName)
    {
        string oldFilePath = Path.Combine(World.WorldDirectory, oldName + ".json");
        string newFilePath = Path.Combine(World.WorldDirectory, newName + ".json");

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
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while renaming file: {e.Message}");
        }
    }
    private void LoadAndDisplayWorlds()
    {
        Transform contentTransform = WorldSelectionPanel.transform.Find("Scroll View/Viewport/Content");

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
        GameObject confirmationScreen = EditWorldPanel.transform.Find("Confirmation").gameObject;

        loadedWorld = World.ReadWorldJSON(worldName);
        int difficultyValue = GetDifficultyValue(loadedWorld.WorldDifficulty);

        SetEditWorldUI(loadedWorld.WorldName, difficultyValue);

        Button saveEditButton = EditWorldPanel.transform.Find("EditScreen/SaveEdit").GetComponent<Button>();
        saveEditButton.onClick.RemoveAllListeners();
        saveEditButton.onClick.AddListener(() => EditTemplate(loadedWorld));

        WorldSelectionPanel.SetActive(false);
        confirmationScreen.SetActive(false);
        EditWorldPanel.SetActive(true);
        EditWorldPanel.transform.Find("EditScreen").gameObject.SetActive(true);
    }

    private void SetEditWorldUI(string worldName, int difficultyValue)
    {
        EditWorldPanel.transform.Find("EditScreen/WorldName").GetComponent<TMP_InputField>().text = worldName;
        EditWorldPanel.transform.Find("EditScreen/Difficulty/Slider").GetComponent<Slider>().value = difficultyValue;
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
        Debug.Log($"World Name: {loadedWorld.WorldName}, " +
            $"World Difficulty: {loadedWorld.WorldDifficulty}, " +
            $"TimesDied: {loadedWorld.TimesDied}, " +
            $"Features: {loadedWorld.Features}");

        FeatureFinder.PrintFeatureInfo(loadedWorld);
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
        GameObject panel = EditWorldPanel.transform.Find(panelName).gameObject;
        panel.SetActive(isActive);
    }

    private void SetConfirmationText(string text)
    {
        TextMeshProUGUI confirmationText = EditWorldPanel.transform.Find("Confirmation/Text").GetComponent<TextMeshProUGUI>();
        confirmationText.text = text;
    }

    private void DeleteWorld()
    {
        if (loadedWorld != null)
        {
            string filePathToDelete = Path.Combine(World.WorldDirectory, loadedWorld.WorldName + ".json");
            if (File.Exists(filePathToDelete))
            {
                File.Delete(filePathToDelete);
                Debug.Log($"World '{loadedWorld.WorldName}' deleted.");

                WorldSelectionPanel.SetActive(true);
                EditWorldPanel.SetActive(false);
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

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
