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
    public int scoreToWin = 3;

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
        Time.timeScale = 1f; // ��ͧ�ѹ������ش�Թ���������� Scene ����
        UpdateScoreUI();
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    void Update()
    {
        Debug.Log("ESC Pressed!");
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
        // ��Ŵ Scene �� (����Ѻ���ҡ˹�� MainMenu)
        SceneManager.LoadScene(gameSceneName);
    }

    public void RestartGame()
    {
        // ��Ŵ Scene �Ѩ�غѹ����������������
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // �Ӥѭ! ��ͧ�׹������ҡ�͹��Ѻ����
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("�͡�ҡ��..."); // ������ Console ����͡�� Editor
        Application.Quit(); // �Դ����� (��ҹ���ԧ����� Build ����)
    }

    public void Resume()
    {
        // ��Ǩ�ͺ����ժ�ͧ��� UI ������� ������������������ ����ͧ Error
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        // ���˹���˹�����ἧ Pause �������Դ���â�������� Error
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

    public void BackToGameScene()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(gameSceneName);
    }
}