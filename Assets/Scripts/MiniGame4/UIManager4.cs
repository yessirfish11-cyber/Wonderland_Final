using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager4 : MonoBehaviour
{
    public static UIManager4 Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Scene")]
    [SerializeField] private string previousSceneName = "MiniGame3Scene";

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
        Debug.Log("[UIManager4] ShowWinPanel called");
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[UIManager4] Win panel shown, timeScale = 0");
        }
        else
        {
            Debug.LogError("[UIManager4] winPanel is NULL!");
        }
    }

    public void ShowLosePanel()
    {
        Debug.Log("[UIManager4] ShowLosePanel called");
        
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[UIManager4] Lose panel shown, timeScale = 0");
        }
        else
        {
            Debug.LogError("[UIManager4] losePanel is NULL!");
        }
    }

    // ═══════════════════════════════════════════
    // ปุ่ม Win Panel
    // ═══════════════════════════════════════════
    public void OnWinBackButton()
    {
        Debug.Log("[UIManager4] OnWinBackButton clicked!");
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }

    // ═══════════════════════════════════════════
    // ปุ่ม Lose Panel
    // ═══════════════════════════════════════════
    public void OnRestartButton()
    {
        Debug.Log("[UIManager4] OnRestartButton clicked!");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnLoseBackButton()
    {
        Debug.Log("[UIManager4] OnLoseBackButton clicked!");
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }
}