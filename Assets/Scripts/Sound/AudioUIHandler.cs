using UnityEngine;
using UnityEngine.UI;

public class AudioUIHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public GameObject checkmarkImage; // ลากรูปติ๊กถูกมาใส่ตรงนี้

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            // 1. ตั้งค่า Slider ตามความดังจริงใน AudioSource
            musicSlider.value = AudioManager.Instance.musicSource.volume;

            // 2. ตั้งค่าติ๊กถูก ตามสถานะ Mute จริง (ถ้าไม่ Mute = มีติ๊กถูก)
            bool isMusicOn = !AudioManager.Instance.musicSource.mute;
            checkmarkImage.SetActive(isMusicOn);

            Debug.Log("UI Refreshed: Volume " + musicSlider.value + " | Music On: " + isMusicOn);
        }
    }

    // เรียกใช้เมื่อเลื่อน Slider (เชื่อมผ่าน OnValueChanged ใน Inspector)
    public void OnMusicSliderChanged()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicSlider.value);
        }
    }

    // เรียกใช้เมื่อกดปุ่มเปิด/ปิด (เชื่อมผ่าน OnClick ใน Inspector)
    public void ToggleMusic()
    {
        if (AudioManager.Instance != null)
        {
            // สลับสถานะ (ถ้าเปิดอยู่ให้ปิด ถ้าปิดอยู่ให้เปิด)
            bool currentIsActive = !AudioManager.Instance.musicSource.mute;
            bool newStatus = !currentIsActive;

            AudioManager.Instance.SetMusicActive(newStatus);

            // อัปเดตรูปติ๊กถูกทันที
            checkmarkImage.SetActive(newStatus);
        }
    }
}
