using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

            // โหลดค่าเริ่มต้น
            musicSource.loop = true;
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            musicSource.mute = PlayerPrefs.GetInt("MusicMute", 0) == 1;
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // ทุกครั้งที่เปลี่ยน Scene ฟังก์ชันนี้จะทำงานอัตโนมัติ!
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupUIReferences();
    }

    public void SetupUIReferences()
    {
        // ค้นหา Slider และ Button ใน Scene ใหม่ด้วย Tag (แม่นยำกว่าชื่อ)
        Slider s = GameObject.FindObjectOfType<Slider>();
        // หมายเหตุ: หากมีหลาย Slider ให้ตั้ง Tag ว่า "MusicSlider" แล้วใช้ GameObject.FindWithTag("MusicSlider")

        // ถ้าหา Slider เจอ ให้เชื่อมต่อทันที
        if (s != null)
        {
            s.onValueChanged.RemoveAllListeners();
            s.value = musicSource.volume;
            s.onValueChanged.AddListener(val => {
                musicSource.volume = val;
                PlayerPrefs.SetFloat("MusicVolume", val);
            });
        }

        // ค้นหา Button เปิด/ปิด
        Button b = GameObject.FindObjectOfType<Button>();
        if (b != null)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => {
                musicSource.mute = !musicSource.mute;
                PlayerPrefs.SetInt("MusicMute", musicSource.mute ? 1 : 0);

                // หา Checkmark ภายใต้ปุ่มนี้แล้วเปิด/ปิด
                Transform check = b.transform.Find("Checkmark"); // เปลี่ยนชื่อตามชื่อ Object ติ๊กถูกของคุณ
                if (check != null) check.gameObject.SetActive(!musicSource.mute);
            });

            // อัปเดตสถานะติ๊กถูกตอนเริ่ม Scene
            Transform c = b.transform.Find("Checkmark");
            if (c != null) c.gameObject.SetActive(!musicSource.mute);
        }
    }
}
