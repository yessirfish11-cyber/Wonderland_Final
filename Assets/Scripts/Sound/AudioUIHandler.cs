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

    [Header("Character Settings")] // เพิ่มส่วนนี้เข้ามา
    public Slider charSlider;
    public Button charToggleButton;
    public GameObject charCheckmark;

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
        if (AudioManager.Instance == null) return;

        // --- ดึงค่าจาก PlayerPrefs มาแสดงผลที่ UI ---
        // ใช้ชื่อ Parameter ให้ตรงกับที่ตั้งไว้ใน AudioManager
        UpdateUISlot("MusicVol", musicSlider, musicCheckmark);
        UpdateUISlot("SFXVol", sfxSlider, sfxCheckmark);
        UpdateUISlot("CharVol", charSlider, charCheckmark);

        SetupListeners();
    }

    // ฟังก์ชันช่วยลดความซ้ำซ้อนในการตั้งค่า UI เบื้องต้น
    void UpdateUISlot(string param, Slider slider, GameObject checkmark)
    {
        if (slider != null)
            slider.value = PlayerPrefs.GetFloat(param, 1.0f);

        if (checkmark != null)
        {
            bool isActive = PlayerPrefs.GetInt(param + "_Mute", 1) == 1;
            checkmark.SetActive(isActive);
        }
    }

    void SetupListeners()
    {
        var am = AudioManager.Instance;

        // Music
        SetupControl(musicSlider, musicToggleButton, musicCheckmark, "MusicVol");

        // SFX
        SetupControl(sfxSlider, sfxToggleButton, sfxCheckmark, "SFXVol");

        // Character
        SetupControl(charSlider, charToggleButton, charCheckmark, "CharVol");
    }

    // ฟังก์ชันรวมการผูก Event เพื่อให้โค้ดดูง่าย
    void SetupControl(Slider slider, Button btn, GameObject checkmark, string param)
    {
        var am = AudioManager.Instance;

        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(val => am.SetVolume(param, val));
        }

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                // อ่านค่าสถานะปัจจุบันจากเครื่องหมายติ๊กถูก
                bool newActiveState = !checkmark.activeSelf;
                am.SetActive(param, newActiveState);
                checkmark.SetActive(newActiveState);

                // เล่นเสียงคลิกทุกครั้งที่มีการกดปุ่ม
                am.PlayClickSound();
            });
        }
    }
}
