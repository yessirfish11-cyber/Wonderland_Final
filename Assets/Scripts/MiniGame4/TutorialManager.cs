using UnityEngine;
using UnityEngine.UI;
public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public Button startButton;

    void Start()
    {
        // 1. ตรวจสอบว่ามี AudioManager ในซีนไหม (เพื่อเล่นเสียงปุ่ม)
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        // 2. เริ่มต้นซีนด้วยการเปิดหน้า Tutorial และหยุดเวลาในเกม
        ShowTutorial();
    }

    void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);

            // หยุดการเคลื่อนที่ทุกอย่างในเกม (FixedUpdate จะไม่ทำงาน)
            Time.timeScale = 0f;
        }
    }

    public void StartGame()
    {
        // 1. เล่นเสียงคลิกผ่าน AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound();
        }

        // 2. ปิดหน้าจอ Tutorial
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 3. ปล่อยให้เวลาในเกมเดินตามปกติ (ตัวละครจะเริ่มเดินได้)
        Time.timeScale = 1f;
    }
}
