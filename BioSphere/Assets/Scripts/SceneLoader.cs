using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen }

public class MainMenuManager : MonoBehaviour
{
    public MenuScreen currentScreen;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;
    public GameObject creatorPanel;
    public GameObject templatePrefab;

    private GameObject GeneralMainFrame;
    private GameObject SoundMainFrame;

    private GameObject WorldSelectionPanel;
    private GameObject CreateWorldPanel;
    private GameObject EditWorldPanel;

    private string input;
    private float slider = 1;

    private void Start()
    {
        WorldSelectionPanel = playPanel.transform.Find("WorldSelection").gameObject;
        CreateWorldPanel = playPanel.transform.Find("CreateWorld").gameObject;
        EditWorldPanel = playPanel.transform.Find("EditWorld").gameObject;
        CreateWorldPanel.SetActive(false);
        WorldSelectionPanel.SetActive(false);
        EditWorldPanel.SetActive(false);

        // Initially, set the main menu as the current screen
        ShowScreen(MenuScreen.MainMenu);
        GameObject generalButton = settingsPanel.transform.Find("General").gameObject;
        GameObject soundButton = settingsPanel.transform.Find("Sound").gameObject;

        GeneralMainFrame = generalButton.transform.Find("Main").gameObject;
        SoundMainFrame = soundButton.transform.Find("Main").gameObject;
    }

    public void ShowMainMenu()
    {
        ShowScreen(MenuScreen.MainMenu);
    }

    public void ShowSettings()
    {
        ShowScreen(MenuScreen.Settings);

        // By default, select the "General" settings
        SelectGeneralSettings();
    }

    public void ShowCredits()
    {
        ShowScreen(MenuScreen.Credits);
    }

    public void ShowPlayScreen()
    {
        ShowScreen(MenuScreen.PlayScreen);

        // Load and display worlds when entering the play screen
        LoadAndDisplayWorlds();
    }

    public void GoBack()
    {
        // Check if CreateWorldPanel is active
        if (CreateWorldPanel.activeSelf || EditWorldPanel.activeSelf)
        {
            // Hide CreateWorldPanel
            CreateWorldPanel.SetActive(false);
            EditWorldPanel.SetActive(false);

            // Show WorldSelectionPanel (PlayMainFrame)
            WorldSelectionPanel.SetActive(true);
        }
        else
        {
            // If CreateWorldPanel is not active, return to the main menu
            ShowMainMenu();
        }
    }


    private void ShowScreen(MenuScreen screen)
    {
        currentScreen = screen;

        // Disable all panels
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        playPanel.SetActive(false);
        creatorPanel.SetActive(false);
        // Enable the panel for the selected screen
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

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    // Method to handle the selection of "General" settings
    public void SelectGeneralSettings()
    {
        GeneralMainFrame.SetActive(true);
        SoundMainFrame.SetActive(false);
    }

    // Method to handle the selection of "Sound" settings
    public void SelectSoundSettings()
    {
        GeneralMainFrame.SetActive(false);
        SoundMainFrame.SetActive(true);
    }

    public void OnCreateButtonPressed()
    {
        // Hide PlayMainFrame
        WorldSelectionPanel.SetActive(false);

        // Show CreateWorldPanel
        CreateWorldPanel.SetActive(true);
    }

    public void ReadStringInput(string s)
    {
        input = s;
    }

    public void ShowSliderValue(float f)
    {
        slider = f;
    }

    public void CreateWorld()
    {
        World writeWorld = new World(
            input,
            (slider == 0) ? "Easy" : (slider == 1) ? "Medium" : "Hard"
        )
        {
            Features = new string[] { "Small fin" }
        };
        //check if name is empty
        if (writeWorld.WorldName != "")
        {
            World.WriteWorldJSON(writeWorld);
        }
    }

    private void EditTemplate(World loadedWorld)
    {
        // Store the old world name
        string oldWorldName = loadedWorld.WorldName;

        // Modify the world data based on user input (new name and difficulty)
        loadedWorld.WorldName = input;
        loadedWorld.WorldDifficulty = (slider == 0) ? "Easy" : (slider == 1) ? "Medium" : "Hard";

        // Rename the JSON file if the world name is changed
        if (oldWorldName != loadedWorld.WorldName)
        {
            string oldFilePath = Path.Combine(World.WorldDirectory, oldWorldName + ".json");
            string newFilePath = Path.Combine(World.WorldDirectory, loadedWorld.WorldName + ".json");
            Debug.Log("in");
            if (File.Exists(oldFilePath))
            {
                File.Move(oldFilePath, newFilePath);
            }
            else
            {
                Debug.LogError($"Error: File {oldFilePath} not found.");
            }
        }

        // Write the updated world data back to the file

        World.WriteWorldJSON(loadedWorld);

        WorldSelectionPanel.SetActive(true);
        EditWorldPanel.SetActive(false);
        LoadAndDisplayWorlds();
    }


    private void LoadAndDisplayWorlds()
    {
        //Debug.Log("loaded");
        Transform contentTransform = WorldSelectionPanel.transform.Find("Scroll View/Viewport/Content");

        // Clear existing world entries
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // Load worlds and instantiate templates
        string[] jsonFiles = Directory.GetFiles(World.WorldDirectory, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            string worldName = Path.GetFileNameWithoutExtension(jsonFile);
            InstantiateWorldEntry(contentTransform, worldName);
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
        GameObject confirmationScreen = playPanel.transform.Find("EditWorld/Confirmation").gameObject;

        World loadedWorld = World.ReadWorldJSON(worldName);
        int difficultyValue = loadedWorld.WorldDifficulty == "Easy" ? 0 : (loadedWorld.WorldDifficulty == "Medium" ? 1 : (loadedWorld.WorldDifficulty == "Hard" ? 2 : -1));

        EditWorldPanel.transform.Find("EditScreen/WorldName").GetComponent<TMP_InputField>().text = loadedWorld.WorldName;
        EditWorldPanel.transform.Find("EditScreen/Difficulty/Slider").GetComponent<Slider>().value = difficultyValue;

        Button saveEditButton = EditWorldPanel.transform.Find("EditScreen/SaveEdit").GetComponent<Button>();
        saveEditButton.onClick.AddListener(() => EditTemplate(loadedWorld));

        // CreateWorld(loadedWorld.WorldName);

        WorldSelectionPanel.SetActive(false);
        confirmationScreen.SetActive(false);
        EditWorldPanel.SetActive(true);
    }

    private void LoadWorld(string worldName)
    {
        // Read the world data from the JSON file
        World loadedWorld = World.ReadWorldJSON(worldName);

        Debug.Log("World Name: " + loadedWorld.WorldName);
        Debug.Log("World Difficulty: " + loadedWorld.WorldDifficulty);
        Debug.Log("TimesDied: " + loadedWorld.TimesDied);

        //Creature.CalcStats(worldName);
    }

}
