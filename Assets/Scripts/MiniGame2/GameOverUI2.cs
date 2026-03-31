using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// จัดการ Game Over UI
///
/// วิธีตั้งค่าใน Inspector:
/// 1. สร้าง Canvas (Screen Space - Overlay)
/// 2. ใน Canvas สร้าง:
///    - GameObject "GameOverPanel" (Image พื้นหลัง) → ลาก gameOverPanel
///    - Button "RestartButton" ใน Panel → ลาก restartButton
/// 3. ลาก Script นี้ไปใส่ใน Canvas หรือ GameObject ใดก็ได้
/// 4. ตั้ง gameOverPanel.SetActive(false) ตั้งแต่ต้น
/// </summary>
public class GameOverUI2 : MonoBehaviour
{
    public static GameOverUI2 Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Fade In")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Scene")]
    [SerializeField] private string sceneToReload = "";  // ว่าง = โหลด Scene ปัจจุบัน

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ซ่อน Panel ตั้งแต่เริ่ม
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    /// <summary>
    /// เรียกจาก GiantMonsterEnemy เมื่อจับผู้เล่นได้
    /// </summary>
    public void Show()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Fade in ถ้ามี CanvasGroup ผูกไว้
        if (panelCanvasGroup != null)
            StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        panelCanvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        string scene = string.IsNullOrEmpty(sceneToReload)
            ? SceneManager.GetActiveScene().name
            : sceneToReload;
        SceneManager.LoadScene(scene);
    }
}