using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum MenuScreen { MainMenu, Settings, Credits, PlayScreen }

public class MainMenuManager : MonoBehaviour
{
    public MenuScreen currentScreen;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject playPanel;

    private GameObject GeneralMainFrame;
    private GameObject SoundMainFrame;

    private GameObject WorldSelectionPanel;
    private GameObject CreateWorldPanel;

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
}
