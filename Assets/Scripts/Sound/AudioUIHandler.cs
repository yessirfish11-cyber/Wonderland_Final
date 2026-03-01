using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioUIHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Button toggleButton;
    public GameObject checkmarkImage;

    private void OnEnable()
    {
        // ทุกครั้งที่เปิดหน้า Setting หรือเปลี่ยน Scene ให้เริ่มค้นหา Manager ใหม่
        StopAllCoroutines();
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        // รอ 0.2 วินาที เพื่อให้ AudioManager ตัวที่ซ้ำซ้อนถูก Destroy ทิ้งไปก่อน
        yield return new WaitForSecondsRealtime(0.2f);

        if (AudioManager.Instance != null)
        {
            // 1. ดึงค่าจริงจาก "Instance ที่รอดชีวิต" มาแสดง
            musicSlider.value = AudioManager.Instance.musicSource.volume;
            checkmarkImage.SetActive(!AudioManager.Instance.musicSource.mute);

            // 2. ล้าง Event เก่า (ป้องกันการอ้างอิงถึง Object ที่ตายไปแล้ว)
            musicSlider.onValueChanged.RemoveAllListeners();
            toggleButton.onClick.RemoveAllListeners();

            // 3. เชื่อมต่อใหม่ผ่าน Code 100%
            musicSlider.onValueChanged.AddListener(OnSliderChanged);
            toggleButton.onClick.AddListener(OnToggleClicked);

            Debug.Log("UI Sync สำเร็จกับ AudioManager: " + AudioManager.Instance.gameObject.name);
        }
    }

    void OnSliderChanged(float val)
    {
        AudioManager.Instance.SetMusicVolume(val);
    }

    void OnToggleClicked()
    {
        bool currentMute = AudioManager.Instance.musicSource.mute;
        bool nextActiveState = currentMute; // ถ้า Mute เป็น True, Active ใหม่ต้องเป็น True

        AudioManager.Instance.SetMusicActive(nextActiveState);
        checkmarkImage.SetActive(nextActiveState);
    }
}
