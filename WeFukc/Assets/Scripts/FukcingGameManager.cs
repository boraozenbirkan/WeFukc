using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FukcingGameManager : MonoBehaviour
{
    [Header("Menu Screen")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private GameObject playerNameInputUI;
    [SerializeField] private GameObject welcomePlayerUI;
    [SerializeField] private TMP_InputField playerNameInput;

    void Start()
    {
        PlayerPrefs.SetString("bossName_1", "Me");
        PlayerPrefs.SetString("bossName_2", "War");
        PlayerPrefs.SetString("bossName_3", "Inflation");
        PlayerPrefs.Save();

        // PlayerPrefs.DeleteKey("playerName"); // Debug only

        if (PlayerPrefs.HasKey("playerName"))   // If player name is already saved
        {
            playerName.text = PlayerPrefs.GetString("playerName");
            welcomePlayerUI.SetActive(true);
        }
        else // If not, get it
        {
            playerNameInputUI.SetActive(true);
        }
    }

    public void GetPlayerName()
    {
        string playerNameEntry = playerNameInput.text;

        PlayerPrefs.SetString("playerName", playerNameEntry); // save it
        PlayerPrefs.Save();
        playerName.text = playerNameEntry;  // write it to text screen

        // open welcome text and close input text
        welcomePlayerUI.SetActive(true);
        playerNameInputUI.SetActive(false);
    }

}
