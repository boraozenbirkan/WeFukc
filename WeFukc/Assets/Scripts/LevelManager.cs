using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Fukcing Level")]
    [SerializeField] private bool fukcingLevel = false;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI bossName;
    [SerializeField] private Camera camera;
    [SerializeField] private float zoomOutSpeed = 1;

    private AudioManager audioManager;
    private bool levelStarted = false;

    void Start()
    {
        if (fukcingLevel)
        {
            // Get the name of the boss
            string levelBoss = "bossName_" + PlayerPrefs.GetFloat("completedLevel").ToString();

            // Display the player name
            if (PlayerPrefs.HasKey("playerName")) playerName.text = PlayerPrefs.GetString("playerName");
            else playerName.text = "Nameless Bastard";

            // Displaye the boss name
            if (PlayerPrefs.HasKey(levelBoss)) bossName.text = PlayerPrefs.GetString(levelBoss);
            else Debug.LogError("There is no boss called: " + levelBoss);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fukcingLevel && levelStarted)
        {
            camera.orthographicSize += zoomOutSpeed * Time.deltaTime;

            // If we zoom out enough, finish the scene and return the main menu
            if (camera.orthographicSize >= 16)
            {
                audioManager.StopSFX("Heavenly");
                audioManager.SetBackgroundMusic(true);
                FindObjectOfType<LevelLoader>().LoadLevel("MenuScene");
            }
        }
    }

    public void LevelStartActions() {
        audioManager = FindObjectOfType<AudioManager>();
        levelStarted = true;

        if (fukcingLevel)
        {
            audioManager.PlaySFX("Heavenly");
        }
    }
}
