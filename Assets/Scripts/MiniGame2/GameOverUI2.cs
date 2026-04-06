using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [SerializeField] private string sceneToReload = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("GameOver"))
                gameOverPanel = obj;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // ล้าง Instance เมื่อ GameObject ถูกทำลาย (Scene unload)
        if (Instance == this)
            Instance = null;
    }

    public void Show()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

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