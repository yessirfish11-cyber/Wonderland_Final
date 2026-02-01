using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool isPaused = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject winPanel;
    
    [Header("Score System")]
    [SerializeField] private TMP_Text scoreText;
    private int currentScore = 0;
    private int scoreToWin = 10;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    void Update()
    {
        // ตรวจสอบการกด ESC
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

    #region Pause Menu Functions
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false); // ปิดหน้า Setting ด้วยถ้าเปิดค้างอยู่
        Time.timeScale = 1f; // ให้เกมเดินต่อ
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกม
        isPaused = true;
    }

    // ฟังก์ชันสำหรับเปิด Setting
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
    #endregion

    #region Score System Functions
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
        
        // Check win condition
        if (currentScore >= scoreToWin)
        {
            Win();
        }
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Maps: " + currentScore + " / " + scoreToWin;
        }
    }
    
    private void Win()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        // หยุดเกม
        Time.timeScale = 0f;
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
    #endregion
}