using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider slider;
    [SerializeField] GameObject loadingObject;
    [SerializeField] GameObject nextSceneButton;

    private bool startScene = true;
    private float progress;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);

    }
    private void Start()
    {
        SetStartScene(true);      // Start loaded when the game starts
    }

    public void LoadLevel(string levelName)
    {
        startScene = false;          // loading just started
        loadingScreen.SetActive(true);  // activate loading screen
        loadingObject.SetActive(true);  // activate loading slider

        StartCoroutine(LoadAsynchronously(levelName)); // Start loading proccess
    }

    IEnumerator LoadAsynchronously(string levelName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);

        while (!operation.isDone)
        {   
            progress = Mathf.Clamp01(operation.progress);
            slider.value = progress;

            yield return null;
        }

        SceneLoaded();
    }

    public void SetStartScene(bool action) { 
        startScene = action; 

        if (startScene) { 
            loadingObject.SetActive(true);      // Open slider again
            nextSceneButton.SetActive(false);   // Close the button
            loadingScreen.SetActive(false);     // Close whole loading screen            
        }
    }

    private void SceneLoaded()
    {
        loadingObject.SetActive(false);     // Remove slider
        nextSceneButton.SetActive(true);    // Place skip button
        Debug.Log("getting here 222");
    }
}
