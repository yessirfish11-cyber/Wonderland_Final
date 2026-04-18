using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputManagerEntry;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer; // ลากไฟล์ Mixer มาใส่ที่นี่

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource characterSource; // เพิ่มส่วนของ Character

    [Header("Audio Clips")]
    public AudioClip clickSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้โค้ดอยู่ข้าม Scene
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- ฟังก์ชันสำหรับเล่นเสียง ---
    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    // --- ฟังก์ชันตั้งค่าระดับเสียง (Slider) ---
    // รับค่า 0.0001 ถึง 1.0 แล้วเปลี่ยนเป็น Decibel (-80 ถึง 0)
    public void SetVolume(string parameterName, float sliderValue)
    {
        float dB = Mathf.Log10(sliderValue) * 20;
        mainMixer.SetFloat(parameterName, dB);
        PlayerPrefs.SetFloat(parameterName, sliderValue);
    }

    // --- ฟังก์ชันเปิด/ปิดเสียง (Toggle) ---
    public void SetActive(string parameterName, bool isActive)
    {
        if (isActive)
        {
            // ถ้าเปิด ให้เอาระดับเสียงล่าสุดจาก PlayerPrefs มาตั้งค่า
            float lastVol = PlayerPrefs.GetFloat(parameterName, 1.0f);
            SetVolume(parameterName, lastVol);
        }
        else
        {
            // ถ้าปิด ให้ปรับ dB ไปที่ -80 (เงียบสนิท)
            mainMixer.SetFloat(parameterName, -80f);
        }
        PlayerPrefs.SetInt(parameterName + "_Mute", isActive ? 1 : 0);
    }

    private void LoadSettings()
    {
        // โหลดค่าความดัง
        LoadParam("MusicVol");
        LoadParam("SFXVol");
        LoadParam("CharVol");
    }

    private void LoadParam(string param)
    {
        float vol = PlayerPrefs.GetFloat(param, 1.0f);
        bool isActive = PlayerPrefs.GetInt(param + "_Mute", 1) == 1;

        if (isActive)
            SetVolume(param, vol);
        else
            mainMixer.SetFloat(param, -80f);
    }
}
