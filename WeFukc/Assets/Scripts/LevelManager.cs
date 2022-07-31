using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Fukcing Level")]
    [SerializeField] private bool fukcingLevel = false;
    [SerializeField] private TextMeshProUGUI flevel_playerName;
    [SerializeField] private TextMeshProUGUI flevel_bossName;
    [SerializeField] private Camera flevel_camera;
    [SerializeField] private float flevel_zoomOutSpeed = 1;


    [Header("Main Menu")]
    [SerializeField] private bool mainMenu = false;
    [SerializeField] private Canvas menu_menuCanvas;
    [SerializeField] private Canvas menu_levelCanvas;
    [SerializeField] private Canvas menu_optionsCanvas;
    [SerializeField] private TextMeshProUGUI menu_playerName;
    [SerializeField] private GameObject menu_playerNameInputUI;
    [SerializeField] private GameObject menu_welcomePlayerUI;
    [SerializeField] private TMP_InputField menu_playerNameInput;
    private bool menu_levelCanvasActive = true;
    private bool menu_optionsCanvasActive = false;
    [SerializeField] private GameObject[] levels;


    private AudioManager audioManager;
    private bool levelStarted = false;

    void Start()
    {
        if (fukcingLevel)
        {
            // Get the name of the boss
            string levelBoss = "bossName_" + PlayerPrefs.GetFloat("completedLevel").ToString();

            // Display the player name
            if (PlayerPrefs.HasKey("playerName")) flevel_playerName.text = PlayerPrefs.GetString("playerName");
            else flevel_playerName.text = "Nameless Bastard";

            // Displaye the boss name
            if (PlayerPrefs.HasKey(levelBoss)) flevel_bossName.text = PlayerPrefs.GetString(levelBoss);
            else Debug.LogError("There is no boss called: " + levelBoss);
        }
        else if (mainMenu)
        {
            // Set names of the bosses
            PlayerPrefs.SetString("bossName_1", "Me");
            PlayerPrefs.SetString("bossName_2", "War");
            PlayerPrefs.SetString("bossName_3", "Inflation");
            PlayerPrefs.SetInt("unlockedLevel_1", 1);   // unlock the level 1 by default
            PlayerPrefs.Save();

            // PlayerPrefs.DeleteKey("playerName"); // Debug only

            DisplayPlayerName();
            CheckLevelAchievements();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fukcingLevel && levelStarted)
        {
            flevel_camera.orthographicSize += flevel_zoomOutSpeed * Time.deltaTime;

            // If we zoom out enough, finish the scene and return the main menu
            if (flevel_camera.orthographicSize >= 16)
            {
                audioManager.StopSFX("Heavenly");
                audioManager.SetBackgroundMusic(true);
                FindObjectOfType<LevelLoader>().LoadLevel("MenuScene");
            }
        }
    }

    //  ***** Fukcing Scene Methods *****   //
    public void LevelStartActions() {
        // Fix stuff that will run in every scene
        audioManager = FindObjectOfType<AudioManager>();
        levelStarted = true;

        if (fukcingLevel)
        {
            audioManager.PlaySFX("Heavenly");
        }
    }



    //  ***** Main Menu Methods *****   //
    public void ToggleLevelCanvas()
    {
        if (menu_levelCanvasActive)   // if menu canvas active, open level canvas, close the menu canvas
        {
            menu_levelCanvas.gameObject.SetActive(false);
            menu_menuCanvas.gameObject.SetActive(true);
            DisplayPlayerName();
        }
        else  // and vice versa
        {
            menu_levelCanvas.gameObject.SetActive(true);
            menu_menuCanvas.gameObject.SetActive(false);
        }

        menu_levelCanvasActive = !menu_levelCanvasActive;
    }
    public void ToggleOptionCanvas()
    {
        if (menu_optionsCanvasActive)   // if options canvas active, open option canvas, close the menu canvas
        {
            menu_optionsCanvas.gameObject.SetActive(false);
            menu_menuCanvas.gameObject.SetActive(true);
            DisplayPlayerName();
        }
        else  // and vice versa
        {
            menu_optionsCanvas.gameObject.SetActive(true);
            menu_menuCanvas.gameObject.SetActive(false);
            CheckLevelAchievements();
        }

        menu_optionsCanvasActive = !menu_optionsCanvasActive;
    }

    // Reset name and level achivements
    public void ResetOptions()
    {
        if (PlayerPrefs.HasKey("playerName")) Debug.Log("Player Name: " + PlayerPrefs.GetString("playerName"));
        else Debug.Log("No name");
        PlayerPrefs.DeleteKey("playerName");    // Delete player name

        // Delete all level achivements
        for (int i = 1; i <= 3; i++)
        {
            string levelName = "unlockedLevel_" + i.ToString();
            PlayerPrefs.DeleteKey(levelName);
        }

        PlayerPrefs.Save();
    }

    // Write player name from data into player prefs
    public void GetPlayerName()
    {
        if (PlayerPrefs.HasKey("playerName") && PlayerPrefs.GetString("playerName") != "") return;  // if the name already written, then don't execute writing code
        
        string playerNameEntry = menu_playerNameInput.text;

        PlayerPrefs.SetString("playerName", playerNameEntry); // save it
        PlayerPrefs.Save();
        menu_playerName.text = playerNameEntry;  // write it to text screen

        // open welcome text and close input text
        menu_welcomePlayerUI.SetActive(true);
        menu_playerNameInputUI.SetActive(false);
    }
    
    // Display updated player name
    private void DisplayPlayerName()
    {
        // If player name is already saved
        if (PlayerPrefs.HasKey("playerName") && PlayerPrefs.GetString("playerName") != "")
        {
            menu_playerName.text = PlayerPrefs.GetString("playerName");
            menu_welcomePlayerUI.SetActive(true);
        }
        else // If not, get it
        {
            menu_playerName.text = ""; // delete if there is any
            menu_playerNameInput.text = "";
            menu_playerNameInputUI.SetActive(true);
        }
    }

    // Check level achivements and update their unlock/lock functionality
    private void CheckLevelAchievements()
    {
        for (int i = 1; i <= 3; i++)
        {
            string levelName = "unlockedLevel_" + i.ToString();

            // if level has been unlocked
            if (PlayerPrefs.HasKey(levelName) && PlayerPrefs.GetInt(levelName) == 1)
            {
                // make its appearance "unlocked"
                levels[i - 1].GetComponentInChildren<Image>().color = new Color(1f, 1f, 1f, 1f);
                // make it interactable
                levels[i - 1].GetComponentInChildren<Button>().interactable = true;
            }
            else
            {
                // make its appearance "locked"
                levels[i - 1].GetComponentInChildren<Image>().color = new Color(1f, 1f, 1f, 0.3f);
                // make it NOT interactable
                levels[i - 1].GetComponentInChildren<Button>().interactable = false;

            }
        }
    }
}
