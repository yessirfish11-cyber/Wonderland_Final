using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager3 : MonoBehaviour
{
    public static UIManager3 Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Scene")]
    [SerializeField] private string previousSceneName = "MiniGame2Scene"; // ชื่อ Scene ก่อนหน้า

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // ปุ่มบน Win Panel — กลับ Scene เดิม
    public void OnWinBackButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }

    // ปุ่มบน Lose Panel — เริ่มใหม่
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ปุ่มบน Lose Panel — กลับ Scene เดิม
    public void OnLoseBackButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }
}