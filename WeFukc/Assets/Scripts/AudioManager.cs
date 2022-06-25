using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // All sound groups shoud be seperate because we should play randomly to have natural outcome
    public Sound[] backgrounMusics;
    public Sound[] attackSFX;
    public Sound[] otherSounds;

    private bool isPlayingMusic;
    private Sound currentPlayingMusic;
    private float delayBetweenMusics = 5f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);

        delayBetweenMusics = 5f;

        foreach (Sound sound in backgrounMusics)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
        }
        foreach (Sound sound in attackSFX)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
        }
        foreach (Sound sound in otherSounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
        }
    }

    private void Start()
    {
        SetBackgroundMusic(true);
    }

    private void Update()
    {
        // If music plays, wait until it ends.
        if (isPlayingMusic)
        {
            if (!currentPlayingMusic.source.isPlaying)  // After it ends, wait a bit more and start new one
            {
                delayBetweenMusics -= Time.deltaTime;
                
                if (delayBetweenMusics <= 0)
                {
                    StartNewBackgroundMusic();
                    delayBetweenMusics = 5f;
                }
            }
        }
    }

    public void PlaySFX(string soundName)
    {
        foreach (Sound sound in otherSounds)
        {
            if (sound.name == soundName)
            {
                sound.source.Play();
                return;
            }
        }
        /*
        Sound s = Array.Find(otherSounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }
        s.source.Play();*/
    }
    public void StopSFX(string soundName)
    {
        Sound sound = Array.Find(otherSounds, item => item.name == soundName);
        if (sound == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        sound.source.volume = sound.volume * (1f + UnityEngine.Random.Range(-sound.volume / 2f, sound.volume / 2f));
        sound.source.pitch = sound.pitch * (1f + UnityEngine.Random.Range(-sound.pitch / 2f, sound.pitch / 2f));

        sound.source.Stop();
    }

    // Start and Stop background musics
    public void SetBackgroundMusic(bool startPlaying)
    {
        if (startPlaying && currentPlayingMusic == null)  // Start BG Music
        {
            // Play random bg music
            Sound music = backgrounMusics[UnityEngine.Random.Range(0, backgrounMusics.Length)];
            music.source.Play();

            isPlayingMusic = true;

            // Save it to previous bg music
            currentPlayingMusic = music;
        }
        else if (currentPlayingMusic != null)   // STOP BG Music
        {
            // Stop the bg music
            currentPlayingMusic.source.Stop();
            currentPlayingMusic = null;
        }
        else Debug.Log("There is no music to stop");
    }

    private void StartNewBackgroundMusic()
    {
        Sound music;
        // Select a random BG Music
        // If it is the same with previous one, then select again
        do
        {
            music = backgrounMusics[UnityEngine.Random.Range(0, backgrounMusics.Length)];
        }
        while (music == currentPlayingMusic);

        // Stop current music again, and start new one
        currentPlayingMusic.source.Stop();
        music.source.Play();

        // Save it
        currentPlayingMusic = music;
    }

    // Play random attack sound
    public void PlayAttackSound()
    {
        Sound attackSound;
        attackSound = attackSFX[UnityEngine.Random.Range(0, attackSFX.Length)];
        attackSound.source.Play();
    }
}
