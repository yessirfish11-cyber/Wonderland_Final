using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioUIHandler : MonoBehaviour
{
    [Header("Music Settings")]
    public Slider musicSlider;
    public Button musicToggleButton;
    public GameObject musicCheckmark;

    [Header("SFX Settings")]
    public Slider sfxSlider;
    public Button sfxToggleButton;
    public GameObject sfxCheckmark;

    // ทำงานตอนโหลด Scene หรือตอนเปิด Object นี้
    private void Start()
    {
        SyncUIWithManager();
    }

    private void OnEnable()
    {
        SyncUIWithManager();
    }

    public void SyncUIWithManager()
    {
        // รอจนกว่า AudioManager จะพร้อม (เผื่อกรณีเปลี่ยน Scene เร็วมาก)
        if (AudioManager.Instance == null) return;

        var am = AudioManager.Instance;

        // --- ดึงค่าจริงจาก Manager มาแสดงที่ UI ของ Scene นี้ ---

        // 1. ตั้งค่า Slider (ต้องทำก่อน AddListener เพื่อไม่ให้เกิด Loop เสียง)
        if (musicSlider != null) musicSlider.value = am.musicSource.volume;
        if (sfxSlider != null) sfxSlider.value = am.sfxSource.volume;

        // 2. ตั้งค่าการติ๊กถูก (Checkmark)
        // !am.musicSource.mute หมายถึง "ถ้าไม่ได้ปิดเสียง ให้โชว์ติ๊กถูก"
        if (musicCheckmark != null) musicCheckmark.SetActive(!am.musicSource.mute);
        if (sfxCheckmark != null) sfxCheckmark.SetActive(!am.sfxSource.mute);

        // 3. ผูก Event ใหม่ (เพื่อความชัวร์ว่าคุมตัวแปรล่าสุด)
        SetupListeners();

        Debug.Log("UI Synced ใน Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void SetupListeners()
    {
        var am = AudioManager.Instance;

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(am.SetMusicVolume);

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(am.SetSFXVolume);

        musicToggleButton.onClick.RemoveAllListeners();
        musicToggleButton.onClick.AddListener(() => {
            bool currentMute = am.musicSource.mute;
            am.SetMusicActive(currentMute); // ส่งค่า mute เดิมเข้าไปเพื่อสลับเป็น active
            musicCheckmark.SetActive(!am.musicSource.mute);
        });

        sfxToggleButton.onClick.RemoveAllListeners();
        sfxToggleButton.onClick.AddListener(() => {
            bool currentMute = am.sfxSource.mute;
            am.SetSFXActive(currentMute);
            sfxCheckmark.SetActive(!am.sfxSource.mute);
        });
    }
}
