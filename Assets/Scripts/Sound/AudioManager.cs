using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource == null) musicSource = GetComponent<AudioSource>();
            LoadSettings();
        }
        else
        {
            // ถ้ามีตัวเดิมอยู่แล้ว ตัวที่เพิ่งเกิดใหม่ต้องรีบทำลายตัวเอง
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetMusicActive(bool isActive)
    {
        musicSource.mute = !isActive;
        PlayerPrefs.SetInt("MusicMute", isActive ? 1 : 0);
    }

    private void LoadSettings()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        musicSource.mute = PlayerPrefs.GetInt("MusicMute", 1) == 0;
    }
}
