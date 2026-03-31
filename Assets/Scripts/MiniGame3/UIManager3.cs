using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager3 : MonoBehaviour
{
    public static UIManager3 Instance;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Heart UI")]
    [SerializeField] private Image[] heartImages;   // ลาก Image 3 อันใส่ตรงนี้
    [SerializeField] private Sprite heartFull;      // Heart01_UI.png (สีแดง)
    [SerializeField] private Sprite heartEmpty;     // BGHeart_UI.png (ใส)

    [Header("Scene")]
    [SerializeField] private string previousSceneName = "MiniGame2Scene";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null)  winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    // เรียกจาก PlayerMiniGame3 ทุกครั้งที่ HP เปลี่ยน
    public void UpdateHearts(int currentLives, int maxLives)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < currentLives) ? heartFull : heartEmpty;
        }
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

    public void OnWinBackButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnLoseBackButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }
}