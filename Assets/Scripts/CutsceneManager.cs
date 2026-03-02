using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // เพิ่มเข้ามาเพื่อคุม Video
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class StoryFrame
{
    public Sprite image;        // ภาพประกอบ
    public VideoClip videoClip;
    [TextArea(3, 10)]
    public string subtitle;     // คำบรรยาย
}

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image displayImage;
    public RawImage videoDisplay;   // Raw Image สำหรับวิดีโอ
    public VideoPlayer videoPlayer; // ตัวเล่นวิดีโอ
    public TextMeshProUGUI subtitleText; // หรือใช้ Text ธรรมดาก็ได้ครับ
    public CanvasGroup fadeGroup;        // ใช้ทำ Effect จางเข้า-ออก

    [Header("Settings")]
    public List<StoryFrame> storyList;   // รายการเนื้อเรื่อง
    public string nextSceneName = "GameScene"; // ชื่อฉากถัดไปที่จะไป
    public float typingSpeed = 0.05f; // ความเร็วในการพิมพ์

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isTyping = false; // ตรวจสอบว่ากำลังพิมพ์อยู่หรือไม่
    private Coroutine typingCoroutine;

    void Start()
    {
        // ซ่อนหน้าจอวิดีโอไว้ก่อนตอนเริ่ม
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);

        if (storyList.Count > 0) StartCoroutine(InitialStart());
    }

    IEnumerator InitialStart()
    {
        isTransitioning = true;
        ShowCurrentFrame(false); // ตั้งค่าภาพแต่ยังไม่พิมพ์
        fadeGroup.alpha = 0;
        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;
        typingCoroutine = StartCoroutine(TypeText(storyList[currentIndex].subtitle));
    }



    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && !isTransitioning)
        {
            if (isTyping)
            {
                // ถ้ากำลังพิมพ์อยู่ แล้วกดปุ่ม ให้แสดงข้อความเต็มทันที
                StopTypingAndShowFullText();
            }
            else
            {
                // ถ้าพิมพ์จบแล้ว ถึงจะไปหน้าถัดไป
                AdvanceStory();
            }
        }
    }

    void AdvanceStory()
    {
        if (currentIndex < storyList.Count - 1)
        {
            currentIndex++;
            StartCoroutine(SwitchFrameRoutine());
        }
        else
        {
            // ถ้าถึงภาพสุดท้ายแล้ว ให้ไปที่ GameScene
            StartCoroutine(EndCutsceneRoutine());
        }
    }

    void ShowCurrentFrame(bool shouldClearText = true)
    {
        StoryFrame current = storyList[currentIndex];

        // ตรวจสอบว่ามีวิดีโอในช่องนี้ไหม
        if (current.videoClip != null)
        {
            displayImage.gameObject.SetActive(false); // ซ่อนภาพนิ่ง
            videoDisplay.gameObject.SetActive(true);  // เปิดหน้าจอวิดีโอ

            videoPlayer.clip = current.videoClip;
            videoPlayer.Play();
        }
        else
        {
            videoDisplay.gameObject.SetActive(false); // ซ่อนหน้าจอวิดีโอ
            displayImage.gameObject.SetActive(true);  // เปิดภาพนิ่ง
            displayImage.sprite = current.image;

            if (videoPlayer.isPlaying) videoPlayer.Stop();
        }

        if (shouldClearText) subtitleText.text = "";
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        subtitleText.text = "";

        // ถ้าคุณมี Script แก้สระลอย ให้จัดการข้อความก่อนเริ่มพิมพ์ตรงนี้
        // string fixedText = ThaiFontAdjuster.Adjust(textToType); 
        // foreach (char letter in fixedText.ToCharArray())

        foreach (char letter in textToType.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void StopTypingAndShowFullText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        subtitleText.text = storyList[currentIndex].subtitle;
        isTyping = false;
    }

    IEnumerator SwitchFrameRoutine()
    {
        isTransitioning = true;

        // Fade Out เฉพาะรูปหรือทั้งหน้าจอ (ตามที่คุณต้องการ)
        yield return StartCoroutine(Fade(1, 0, 0.5f));

        ShowCurrentFrame();

        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;
        // เริ่มพิมพ์หลังจาก Fade In เสร็จ
        typingCoroutine = StartCoroutine(TypeText(storyList[currentIndex].subtitle));
    }

    IEnumerator EndCutsceneRoutine()
    {
        isTransitioning = true;
        if (fadeGroup != null) yield return StartCoroutine(Fade(1, 0, 1f));

        if (GameManager.Instance != null) GameManager.Instance.FinishCutscene();
        else SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        fadeGroup.alpha = endAlpha;
    }
}
