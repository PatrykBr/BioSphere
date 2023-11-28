using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen }

public class MainMenuManager : MonoBehaviour
{
    public MenuScreen currentScreen;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;
    public GameObject templatePrefab;

    private GameObject GeneralMainFrame;
    private GameObject SoundMainFrame;

    private GameObject WorldSelectionPanel;
    private GameObject CreateWorldPanel;

    private string input;
    private float slider = 1;

    private void Start()
    {
        WorldSelectionPanel = playPanel.transform.Find("Bg").Find("WorldSelection").gameObject;
        CreateWorldPanel = playPanel.transform.Find("Bg").Find("CreateWorld").gameObject;
        CreateWorldPanel.SetActive(false);
        WorldSelectionPanel.SetActive(false);

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
        if (CreateWorldPanel.activeSelf)
        {
            // Hide CreateWorldPanel
            CreateWorldPanel.SetActive(false);

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

    private void LoadAndDisplayWorlds()
    {
        Transform contentTransform = WorldSelectionPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");

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
    }


    private void LoadWorld(string worldName)
    {
        // Read the world data from the JSON file
        World loadedWorld = World.ReadWorldJSON(worldName);

        Debug.Log("World Name: " + loadedWorld.WorldName);
        Debug.Log("World Difficulty: " + loadedWorld.WorldDifficulty);
        Debug.Log("TimesDied: " + loadedWorld.TimesDied);

        Creature.CalcStats(worldName);
    }

}
