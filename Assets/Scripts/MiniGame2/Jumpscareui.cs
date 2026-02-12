using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// จัดการ Jump Scare และปุ่ม Restart หลังโดนจับ
/// 
/// วิธีตั้งค่าใน Inspector:
/// 1. สร้าง Canvas (Screen Space - Overlay)
/// 2. ใน Canvas สร้าง:
///    - Image (jumpScareImage)     → รูป Jump Scare เต็มจอ, alpha=0 ตั้งต้น
///    - Image (blackOverlay)       → สีดำเต็มจอ, alpha=0 ตั้งต้น
///    - Button (restartButton)     → ปุ่ม Restart, alpha=0 ตั้งต้น
///    - AudioSource                → สำหรับเสียง Jump Scare
/// 3. ลาก Component นี้ไปใส่ใน Canvas GameObject
/// 4. ผูก Reference ทั้งหมดใน Inspector
/// </summary>
public class JumpScareUI : MonoBehaviour
{
    public static JumpScareUI Instance { get; private set; }

    [Header("Jump Scare Image")]
    [SerializeField] private Image jumpScareImage;
    [SerializeField] private float jumpScareDisplayTime = 1.5f;   // แสดงรูปนานแค่ไหน
    [SerializeField] private float jumpScareInDuration = 0.05f;   // Fade-in เร็วมาก = ตกใจ
    [SerializeField] private float jumpScareOutDuration = 0.4f;   // Fade-out ช้ากว่า

    [Header("Black Overlay")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private float overlayFadeDuration = 0.6f;    // Fade ดำหลัง Jump Scare

    [Header("Restart Button")]
    [SerializeField] private Button restartButton;
    [SerializeField] private CanvasGroup restartButtonGroup;       // CanvasGroup บน Button สำหรับ Fade
    [SerializeField] private float restartButtonDelay = 0.5f;     // รอก่อน Fade ปุ่มขึ้น
    [SerializeField] private float restartButtonFadeDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource jumpScareAudio;
    [SerializeField] private AudioClip jumpScareSound;

    [Header("Scene")]
    [SerializeField] private string sceneToReload = "";           // ว่าง = โหลด Scene ปัจจุบัน

    // ============================================================

    private void Awake()
    {
        // Singleton ง่ายๆ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ซ่อนทุกอย่างตั้งแต่ต้น
        SetAlpha(jumpScareImage, 0f);
        SetAlpha(blackOverlay, 0f);

        if (restartButtonGroup != null)
        {
            restartButtonGroup.alpha = 0f;
            restartButtonGroup.interactable = false;
            restartButtonGroup.blocksRaycasts = false;
        }

        // ผูกปุ่ม Restart
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    // ============================================================
    // Public API — เรียกจาก GiantMonsterEnemy เมื่อจับผู้เล่นได้
    // ============================================================

    /// <summary>
    /// เริ่มลำดับ Jump Scare + Fade ปุ่ม Restart
    /// เรียกจาก GiantMonsterEnemy.AttackSequence()
    /// </summary>
    public void TriggerJumpScare()
    {
        StartCoroutine(JumpScareSequence());
    }

    // ============================================================
    // Coroutine หลัก
    // ============================================================

    private IEnumerator JumpScareSequence()
    {
        // ── 1. เสียง Jump Scare ──────────────────────────────────
        if (jumpScareAudio != null && jumpScareSound != null)
            jumpScareAudio.PlayOneShot(jumpScareSound);

        // ── 2. Fade In รูป Jump Scare (เร็วมาก) ─────────────────
        yield return StartCoroutine(FadeImage(jumpScareImage, 0f, 1f, jumpScareInDuration));

        // ── 3. แสดงรูปค้างไว้ ────────────────────────────────────
        yield return new WaitForSecondsRealtime(jumpScareDisplayTime);

        // ── 4. Fade Out รูป + Fade In Black Overlay พร้อมกัน ────
        yield return StartCoroutine(FadeOutScareAndInOverlay());

        // ── 5. รอนิดนึงก่อนโชว์ปุ่ม ──────────────────────────────
        yield return new WaitForSecondsRealtime(restartButtonDelay);

        // ── 6. Fade ปุ่ม Restart ขึ้นมา ─────────────────────────
        yield return StartCoroutine(FadeInRestartButton());
    }

    // ─────────────────────────────────────────────────────────────
    // Helper Coroutines
    // ─────────────────────────────────────────────────────────────

    private IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        SetAlpha(img, from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;   // ใช้ UnscaledTime เผื่อ TimeScale = 0
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(img, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetAlpha(img, to);
    }

    private IEnumerator FadeOutScareAndInOverlay()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(jumpScareOutDuration, overlayFadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Jump Scare Fade Out
            if (jumpScareImage != null)
            {
                float scareT = Mathf.Clamp01(elapsed / jumpScareOutDuration);
                SetAlpha(jumpScareImage, Mathf.Lerp(1f, 0f, scareT));
            }

            // Black Overlay Fade In
            if (blackOverlay != null)
            {
                float overlayT = Mathf.Clamp01(elapsed / overlayFadeDuration);
                SetAlpha(blackOverlay, Mathf.Lerp(0f, 1f, overlayT));
            }

            yield return null;
        }

        SetAlpha(jumpScareImage, 0f);
        SetAlpha(blackOverlay, 1f);
    }

    private IEnumerator FadeInRestartButton()
    {
        if (restartButtonGroup == null) yield break;

        restartButtonGroup.interactable = false;
        restartButtonGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < restartButtonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            restartButtonGroup.alpha = Mathf.Clamp01(elapsed / restartButtonFadeDuration);
            yield return null;
        }

        restartButtonGroup.alpha = 1f;
        restartButtonGroup.interactable = true;
        restartButtonGroup.blocksRaycasts = true;
    }

    // ─────────────────────────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────────────────────────

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f; // คืนค่า TimeScale ก่อน Load Scene
        string scene = string.IsNullOrEmpty(sceneToReload)
            ? SceneManager.GetActiveScene().name
            : sceneToReload;
        SceneManager.LoadScene(scene);
    }
}