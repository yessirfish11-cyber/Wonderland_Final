using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip clickSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMusicActive(bool isActive)
    {
        musicSource.mute = !isActive;
        PlayerPrefs.SetInt("MusicMute", isActive ? 1 : 0);
    }

    public void SetSFXActive(bool isActive)
    {
        sfxSource.mute = !isActive;
        PlayerPrefs.SetInt("SFXMute", isActive ? 1 : 0);
    }

    private void LoadSettings()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        musicSource.mute = PlayerPrefs.GetInt("MusicMute", 1) == 0;
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        sfxSource.mute = PlayerPrefs.GetInt("SFXMute", 1) == 0;
    }
}
