using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager.
/// Handles Win state and UI.
/// 
/// วิธีผูกปุ่ม Back มี 2 แบบ เลือกแบบใดแบบหนึ่ง:
///   แบบ A) ลาก Button ใส่ช่อง backButton ใน Inspector (โค้ดผูก onClick ให้อัตโนมัติ)
///   แบบ B) ใน Inspector ของ Button → OnClick → ลาก GameManager → เลือก GoBack()
/// </summary>
public class GameManagerMiniGame5 : MonoBehaviour
{
    public static GameManagerMiniGame5 Instance;

    [Header("Win UI")]
    public GameObject winPanel;
    public Button backButton;

    [Header("Scene to return to")]
    [Tooltip("ใส่ชื่อ Scene ที่ต้องการกลับ ต้องอยู่ใน File > Build Settings")]
    public string previousSceneName = "";

    [Tooltip("ถ้าไม่รู้ชื่อ Scene ให้ใส่ Index แทน (0 = Scene แรกใน Build Settings)")]
    public int previousSceneIndex = 0;

    [Tooltip("true = ใช้ Index / false = ใช้ชื่อ")]
    public bool useSceneIndex = false;

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        // ผูก onClick อัตโนมัติ (แบบ A)
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }
    }

    public void TriggerWin()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("=== YOU WIN! ===");

        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm != null) wm.StopWaveCycle();

        PlayerControllerMiniGame5 player = FindFirstObjectByType<PlayerControllerMiniGame5>();
        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // เรียกจากโค้ด หรือผูกตรงใน Inspector ของ Button ก็ได้ (แบบ B)
    public void GoBack()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(previousSceneName);
    }
}