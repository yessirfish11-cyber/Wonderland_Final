using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // สำคัญมากสำหรับการเปลี่ยนฉาก
using System.Collections;
using System.Collections.Generic;
using TMPro;


[System.Serializable]
public class StoryFrame
{
    public Sprite image;        // ภาพประกอบ
    [TextArea(3, 10)]
    public string subtitle;     // คำบรรยาย
}

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image displayImage;
    public TextMeshProUGUI subtitleText; // หรือใช้ Text ธรรมดาก็ได้ครับ
    public CanvasGroup fadeGroup;        // ใช้ทำ Effect จางเข้า-ออก

    [Header("Settings")]
    public List<StoryFrame> storyList;   // รายการเนื้อเรื่อง
    public string nextSceneName = "GameScene"; // ชื่อฉากถัดไปที่จะไป

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (storyList.Count > 0)
        {
            ShowCurrentFrame();
        }
    }

    void Update()
    {
        // รับ Input จากผู้เล่น
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && !isTransitioning)
        {
            AdvanceStory();
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

    void ShowCurrentFrame()
    {
        displayImage.sprite = storyList[currentIndex].image;
        subtitleText.text = storyList[currentIndex].subtitle;
    }

    IEnumerator SwitchFrameRoutine()
    {
        isTransitioning = true;

        // ค่อยๆ จางหาย (Fade Out)
        yield return StartCoroutine(Fade(1, 0, 0.5f));

        ShowCurrentFrame();

        // ค่อยๆ ปรากฏ (Fade In)
        yield return StartCoroutine(Fade(0, 1, 0.5f));

        isTransitioning = false;
    }

    IEnumerator EndCutsceneRoutine()
    {
        isTransitioning = true;

        // 1. ตรวจสอบว่ามี Fade Group ไหม ถ้ามีให้ทำ Fade Out
        if (fadeGroup != null)
        {
            yield return StartCoroutine(Fade(1, 0, 1f));
        }

        Debug.Log("จบ CutScene กำลังเตรียมโหลดฉากถัดไป...");

        // 2. เรียกใช้ GameManager เพื่อเปลี่ยนฉาก
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishCutscene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
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
