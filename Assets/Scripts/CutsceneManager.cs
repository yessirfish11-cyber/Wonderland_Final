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
    public string characterName;
    public Sprite background;
    public Sprite mainImage;
    public Sprite secondaryImage;
    public VideoClip videoClip;
    public List<AudioClip> voiceOverList;      // <-- เพิ่มช่องสำหรับใส่เสียงในแต่ละ Frame
    [TextArea(3, 10)]
    public string subtitle;
}


public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public Image backgroundImage;
    public Image displayImage;
    public Image secondaryDisplay;   // <--- เพิ่ม Image สำหรับแสดงผล secondaryImage
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

    [Header("New UI Buttons")]
    public Button continueButton; // ปุ่มสำหรับไปต่อ
    public Button skipButton;     // ปุ่มสำหรับข้ามทั้งหมด

    void Start()
    {
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (storyList.Count > 0) StartCoroutine(InitialStart());

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
            continueButton.gameObject.SetActive(true); // มั่นใจว่าเปิดใช้งานอยู่ตลอด
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipAll);

        if (storyList.Count > 0) StartCoroutine(InitialStart());
    }

    // ฟังก์ชันเมื่อกดปุ่ม Continue หรือคลิกจอ
    public void OnContinuePressed()
    {
        if (isTransitioning) return;

        // หยุด Coroutine ทั้งหมดที่เกี่ยวกับเฟรมเก่า (เสียงพากย์ และ การพิมพ์ข้อความ)
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (audioQueueCoroutine != null) StopCoroutine(audioQueueCoroutine);
        audioSource.Stop();

        // ไปยังเนื้อเรื่องส่วนต่อไป
        AdvanceStory();
    }

    // ฟังก์ชันสำหรับปุ่ม Skip
    public void SkipAll()
    {
        if (isTransitioning) return;
        StartCoroutine(EndCutsceneRoutine());
    }

    // แก้ไข Update เล็กน้อยเพื่อให้ปุ่ม Space ทำงานเหมือนปุ่ม Continue
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            OnContinuePressed();
        }
    }

    IEnumerator InitialStart()
    {
        isTransitioning = true;
        ShowCurrentFrame(false);
        fadeGroup.alpha = 0;
        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;

        // เริ่มเล่นคิวเสียงและพิมพ์ข้อความ
        UpdateCharacterName();
        audioQueueCoroutine = StartCoroutine(PlayAudioQueue());
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(storyList[currentIndex].subtitle));
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

    void UpdateCharacterName()
    {
        // อัปเดตชื่อตัวละครในแต่ละเฟรม
        if (nameText != null)
        {
            nameText.text = storyList[currentIndex].characterName;
        }
    }

    void ShowCurrentFrame(bool shouldClearText = true)
    {
        StoryFrame current = storyList[currentIndex];
        UpdateCharacterName();

        // 1. จัดการ Background
        if (current.background != null)
        {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.sprite = current.background;
        }

        // 2. จัดการ Content (Video หรือ Images)
        if (current.videoClip != null)
        {
            displayImage.gameObject.SetActive(false);
            secondaryDisplay.gameObject.SetActive(false); // ปิดภาพที่สองถ้าเล่นวิดีโอ
            videoDisplay.gameObject.SetActive(true);
            videoPlayer.clip = current.videoClip;
            videoPlayer.Play();
        }
        else
        {
            videoDisplay.gameObject.SetActive(false);
            if (videoPlayer.isPlaying) videoPlayer.Stop();

            // จัดการรูปที่ 1
            if (current.mainImage != null)
            {
                displayImage.gameObject.SetActive(true);
                displayImage.sprite = current.mainImage;
            }
            else
            {
                displayImage.gameObject.SetActive(false);
            }

            // จัดการรูปที่ 2 (เงื่อนไขเดียวกับรูปแรก)
            if (current.secondaryImage != null)
            {
                secondaryDisplay.gameObject.SetActive(true);
                secondaryDisplay.sprite = current.secondaryImage;
            }
            else
            {
                secondaryDisplay.gameObject.SetActive(false);
            }
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
        subtitleText.text = ""; // <-- ต้องล้างข้อความเดิมออกทุกครั้งที่เริ่มฟังก์ชัน!

        foreach (char letter in textToType.ToCharArray())
        {
            subtitleText.text += letter; // ค่อยๆ เพิ่มตัวอักษร
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
