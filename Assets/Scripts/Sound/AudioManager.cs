using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    private void Awake()
    {
        // ระบบ Singleton: ทำให้มี AudioManager เพียงตัวเดียวในทุก Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings(); // โหลดค่าที่บันทึกไว้
        }
        else
        {
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
        // musicSource.mute = true หมายถึงปิดเสียง
        // ดังนั้นถ้า isActive เป็น true (เปิดเสียง) mute ต้องเป็น false
        musicSource.mute = !isActive;
        PlayerPrefs.SetInt("MusicMute", isActive ? 1 : 0);
    }

    private void LoadSettings()
    {
        // ใช้ค่าเริ่มต้น 1.0f (เสียงดังสุด) ถ้ายังไม่เคยบันทึก
        float vol = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        musicSource.volume = vol;

        // 1 = เปิดเสียง, 0 = ปิดเสียง (เริ่มต้นให้เป็น 1)
        bool isActive = PlayerPrefs.GetInt("MusicMute", 1) == 1;
        musicSource.mute = !isActive;
    }
}
