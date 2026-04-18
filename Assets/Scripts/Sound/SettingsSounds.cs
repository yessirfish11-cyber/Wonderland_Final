using UnityEngine;
using UnityEngine.UI;

public class SettingsSounds : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Toggle musicToggle;
    public GameObject musicCheckmark; // ลากภาพติ๊กถูกมาใส่

    void Start()
    {
        // ดึงค่าเก่ามาแสดงผลที่ UI
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1.0f);
        musicToggle.isOn = PlayerPrefs.GetInt("MusicVol_Mute", 1) == 1;
        musicCheckmark.SetActive(musicToggle.isOn);

        // ผูก Event
        musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetVolume("MusicVol", val));
        musicToggle.onValueChanged.AddListener(val =>
        {
            AudioManager.Instance.SetActive("MusicVol", val);
            musicCheckmark.SetActive(val); // เปิด/ปิดติ๊กถูก
        });
    }
}