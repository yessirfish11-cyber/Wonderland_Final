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

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";

    void Awake()
    {
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
        Time.timeScale = 1f; // ป้องกันเวลาหยุดเดินเมื่อเริ่ม Scene ใหม่
        UpdateScoreUI();
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    void Update()
    {
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

    public void PlayGame()
    {
        // โหลด Scene เกม (สำหรับกดจากหน้า MainMenu)
        SceneManager.LoadScene(gameSceneName);
    }

    public void RestartGame()
    {
        // โหลด Scene ปัจจุบันซ้ำเพื่อเริ่มใหม่
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // สำคัญ! ต้องคืนค่าเวลาก่อนกลับเมนู
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("ออกจากเกม..."); // จะเห็นใน Console เมื่อกดใน Editor
        Application.Quit(); // ปิดโปรแกรม (ใช้งานได้จริงเมื่อ Build แล้ว)
    }

    public void Resume()
    {
        // ตรวจสอบว่ามีช่องใส่ UI หรือไม่ ถ้าไม่มีให้ข้ามไปเลย ไม่ต้อง Error
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        // ถ้าหน้าไหนไม่มีแผง Pause ก็จะไม่เกิดอะไรขึ้นและไม่ Error
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();

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
}