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
    public Sprite background;
    public Sprite image;
    public VideoClip videoClip;
    public List<AudioClip> voiceOverList;      // <-- เพิ่มช่องสำหรับใส่เสียงในแต่ละ Frame
    [TextArea(3, 10)]
    public string subtitle;
}


public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image backgroundImage;
    public Image displayImage;
    public RawImage videoDisplay;   // Raw Image สำหรับวิดีโอ
    public VideoPlayer videoPlayer; // ตัวเล่นวิดีโอ
    public TextMeshProUGUI subtitleText; // หรือใช้ Text ธรรมดาก็ได้ครับ
    public CanvasGroup fadeGroup;        // ใช้ทำ Effect จางเข้า-ออก

    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Settings")]
    public List<StoryFrame> storyList;   // รายการเนื้อเรื่อง
    public string nextSceneName = ""; // ชื่อฉากถัดไปที่จะไป
    public float typingSpeed = 0.05f; // ความเร็วในการพิมพ์

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isTyping = false; // ตรวจสอบว่ากำลังพิมพ์อยู่หรือไม่
    private Coroutine typingCoroutine;
    private Coroutine audioQueueCoroutine; // สำหรับควบคุมคิวเสียง

    void Start()
    {
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (storyList.Count > 0) StartCoroutine(InitialStart());
    }

    IEnumerator InitialStart()
    {
        isTransitioning = true;
        ShowCurrentFrame(false);
        fadeGroup.alpha = 0;
        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;

        // เริ่มเล่นคิวเสียงและพิมพ์ข้อความ
        audioQueueCoroutine = StartCoroutine(PlayAudioQueue());
        typingCoroutine = StartCoroutine(TypeText(storyList[currentIndex].subtitle));
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && !isTransitioning)
        {
            if (isTyping)
            {
                StopTypingAndShowFullText();
            }
            else
            {
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
            StartCoroutine(EndCutsceneRoutine());
        }
    }

    void ShowCurrentFrame(bool shouldClearText = true)
    {
        StoryFrame current = storyList[currentIndex];

        // 1. จัดการ Background
        if (current.background != null)
        {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.sprite = current.background;
        }
        else
        {
            // ถ้าเฟรมนี้ไม่มี Background อาจจะเลือกปิดไปหรือปล่อยทิ้งไว้ตามความเหมาะสม
            // backgroundImage.gameObject.SetActive(false); 
        }

        // 2. จัดการ Content (Video หรือ Image)
        if (current.videoClip != null)
        {
            displayImage.gameObject.SetActive(false);
            videoDisplay.gameObject.SetActive(true);
            videoPlayer.clip = current.videoClip;
            videoPlayer.Play();
        }
        else
        {
            videoDisplay.gameObject.SetActive(false);

            // ถ้าไม่มีรูปประกอบหลัก (image) ให้ปิดตัวละครไป แต่ถ้ามีก็แสดงผล
            if (current.image != null)
            {
                displayImage.gameObject.SetActive(true);
                displayImage.sprite = current.image;
            }
            else
            {
                displayImage.gameObject.SetActive(false);
            }

            if (videoPlayer.isPlaying) videoPlayer.Stop();
        }

        if (shouldClearText) subtitleText.text = "";
    }

    // ฟังก์ชันใหม่: เล่นเสียงเรียงตามลำดับใน List
    IEnumerator PlayAudioQueue()
    {
        List<AudioClip> clips = storyList[currentIndex].voiceOverList;

        if (clips != null && clips.Count > 0)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;

                audioSource.clip = clip;
                audioSource.Play();

                // รอจนกว่าเสียงปัจจุบันจะเล่นจบ ก่อนจะวนไปเล่นไฟล์ถัดไป
                yield return new WaitWhile(() => audioSource.isPlaying);

                // เว้นช่วงเล็กน้อยระหว่างเสียง (ถ้าต้องการ)
                // yield return new WaitForSeconds(0.2f); 
            }
        }
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        subtitleText.text = "";
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

        // หยุดเสียงและคิวเสียงเดิมทันทีเมื่อเปลี่ยนเฟรม
        if (audioQueueCoroutine != null) StopCoroutine(audioQueueCoroutine);
        audioSource.Stop();

        yield return StartCoroutine(Fade(1, 0, 0.5f));

        ShowCurrentFrame();

        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;

        // เริ่มคิวเสียงและเริ่มพิมพ์ใหม่ในเฟรมถัดไป
        audioQueueCoroutine = StartCoroutine(PlayAudioQueue());
        typingCoroutine = StartCoroutine(TypeText(storyList[currentIndex].subtitle));
    }

    IEnumerator EndCutsceneRoutine()
    {
        isTransitioning = true;
        if (audioQueueCoroutine != null) StopCoroutine(audioQueueCoroutine);
        audioSource.Stop();

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
