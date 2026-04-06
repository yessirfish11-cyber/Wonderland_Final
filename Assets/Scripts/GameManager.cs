using UnityEngine;
using UnityEngine.SceneManagement;
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
    public int scoreToWin = 3;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string cutsceneSceneName = "CutsceneScene";
    [SerializeField] private string gameSceneName = "GameScene";

    void Awake()
    {
        // ไม่ต้องมี DontDestroyOnLoad เลย
        // แต่ละ Scene สร้าง GameManager ใหม่เสมอ
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // หาทุก GameObject ที่มี Tag นี้ แม้ซ่อนอยู่
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("WinPanel"))
                winPanel = obj;
        }

        if (winPanel != null)
            winPanel.SetActive(false);

        UpdateScoreUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void PlayGame()      => SceneManager.LoadScene(cutsceneSceneName);

    public void FinishCutscene()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToGameScene()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
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
            Win();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Maps: " + currentScore + " / " + scoreToWin;
    }

    public void Win()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public int GetCurrentScore() => currentScore;

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
}