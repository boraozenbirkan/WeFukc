using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Elevator : MonoBehaviour
{
    [SerializeField] private Image endScreen;
    [SerializeField] private TextMeshProUGUI endScreenText;
    [SerializeField] private TextMeshProUGUI keyWarningText;
    private bool turnOnEndScreen = false;
    private float opacity = 0f;

    private void Update()
    {
        if (turnOnEndScreen) {
            if (opacity < 254f) { 
                opacity += Time.deltaTime / 5f;
                endScreen.color = new Color(0f, 0f, 0f, opacity);
                endScreenText.color = new Color(255f, 255f, 255f, opacity); 
            }
        }
    }

    public void ShowKeyWarning()
    {
        StartCoroutine(KeyWarning());
    }
    IEnumerator KeyWarning()
    {
        keyWarningText.enabled = true;
        yield return new WaitForSeconds(5f);
        keyWarningText.enabled = false;
    }


    public void ActivateElevator()
    {
        // Start moving down
        GetComponent<Rigidbody2D>().gravityScale = 0.2f;

        // Stop background music and play elevator music
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        audioManager.SetBackgroundMusic(false);
        audioManager.isBbackgroundMusicOn = false;
        audioManager.PlaySFX("Elevator");

        turnOnEndScreen = true;
    }
}
