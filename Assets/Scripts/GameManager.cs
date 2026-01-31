using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;

    void Update()
    {
        // ตรวจสอบการกดปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false); // ปิดหน้า Setting ด้วยถ้าเปิดค้างไว้
        Time.timeScale = 1f; // ให้เวลาในเกมเดินปกติ
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกมทั้งหมด
        isPaused = true;
    }

    // ฟังก์ชันสำหรับปุ่ม Setting
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    // ฟังก์ชันสำหรับปุ่ม Back ในหน้า Setting
    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}
