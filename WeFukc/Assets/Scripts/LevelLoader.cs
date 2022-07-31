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
    [SerializeField] TMPro.TextMeshProUGUI loadingText;

    // Loading Texts
    private string[] loadingTexts = {
        "Did you know that Satoshi Nakamoto, the  **The Times 03/Jan/2009 Chancellor on brink of second bailout for banks.**. So what does it mean?",
        "As of March 2021, COVID costs totaled $5.2 trillion. World War II cost $4.7 trillion (in today’s dollars). Can you see how worthless fiat money become?",
        "When goverments and banks print money, they basically take your money from your pocket. Because they are decreasing value of the money by increasing the supply.",
        "In 2008, governments rescued the bank from bankruptcy. Even though banks cause the crisis, they did not pay the cost, you did.",
        "Did you know that private banks print money? Not only governments and central banks, but private banks print money also",
        "Who the fukc are you?",
        "Do you know what fractional reserve banking means? This is the system all banks use. What it means is, that they only hold a fraction of the reserves they suppose to hold. Therefore, if everyone tries to get withdraw their money, only elites can get it. Even elites can not get their all money!",
        "Do you know what is lobbying? In simple terms, corporations finance politicians, and politicians make laws in favor of those corporations. Yes, it is legal and quite common.",
        "Freedom of speech? Free market? Freedom? Do you believe they are real, right? Ahaha you are so naive!",
        "Rights are not granted, they are taken. If you want your rights, you have to take them!",
        "Agustin Carstens, the General Manager of BIS (the bank of all central banks) said **Central banks will have absolute control over how money is spent!** for their own digital money model, CDBC!",
        "Learn why Bitcoin is invented!",
        "Be aware of your potential!",
        "Respect yourself and stand for your rights!"
    };

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
        CloseLoadingScreen();      // Start loaded when the game starts
    }

    ///     ************        ///
    ///    Private Methods      ///       
    ///     ************        ///

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

    private void SceneLoaded()
    {
        loadingObject.SetActive(false);     // Remove slider
        nextSceneButton.SetActive(true);    // Place skip button
    }

    ///     ************        ///
    ///    Public Methods       ///       
    ///     ************        ///
    
    
    public void LoadLevel(string levelName)
    {
        loadingScreen.SetActive(true);  // activate loading screen
        loadingObject.SetActive(true);  // activate loading slider

        // Give random text for loading screen
        loadingText.text = loadingTexts[Random.Range(0, loadingTexts.Length)];  

        StartCoroutine(LoadAsynchronously(levelName)); // Start loading proccess
    }
    public void CloseLoadingScreen()
    {
        loadingObject.SetActive(true);      // Open slider again
        nextSceneButton.SetActive(false);   // Close the button
        loadingScreen.SetActive(false);     // Close whole loading screen

        FindObjectOfType<LevelManager>().LevelStartActions();   // trigger the level start action in the following scene
    }
    public bool isSceneReady()
    {
        // if loading screen is active, then scene is not ready
        if (loadingScreen.activeSelf) return false;
        else return true;
    }
}
