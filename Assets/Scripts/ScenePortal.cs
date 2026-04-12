using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // หากใช้ TextMeshPro อย่าลืมเพิ่มตัวนี้ครับ
using System.Collections;

public class ScenePortal : MonoBehaviour
{
    [Header("Mode Settings")]
    public bool useDialogue = true;      // ติ๊กออกถ้าต้องการโชว์แค่ Tutorial

    [Header("Transition Settings")]
    public GameObject transitionCanvas;
    public string nextSceneName;

    [Header("Dialogue Content (Optional)")]
    public string characterName;
    [TextArea(3, 10)]
    public string dialogueContent;
    public float typingSpeed = 0.05f;

    [Header("UI Components")]
    public GameObject dialogueBox;       // ลาก Parent ของชุดข้อความมาใส่ (เพื่อซ่อน/โชว์)
    public GameObject tutorialImage;     // ลากรูปภาพ Tutorial มาใส่
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public Button nextButton;

    private bool isActivated = false;

    private void Start()
    {
        // ปิด UI ทั้งหมดตอนเริ่มเกม
        if (transitionCanvas != null) transitionCanvas.SetActive(false);

        // ผูก Event ให้ปุ่มแค่ครั้งเดียวตอนเริ่ม
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners(); // ล้าง Event เก่ากันพลาด
            nextButton.onClick.AddListener(ChangeScene);
            nextButton.gameObject.SetActive(false); // เริ่มต้นปิดไว้ก่อน
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            ShowTransitionUI();
        }
    }

    void ShowTransitionUI()
    {
        Time.timeScale = 0f;
        if (transitionCanvas != null) transitionCanvas.SetActive(true);

        if (useDialogue)
        {
            // --- โหมดมี Dialogue ---
            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (tutorialImage != null) tutorialImage.SetActive(true);

            if (nameText != null) nameText.text = characterName;
            if (contentText != null) contentText.text = "";

            StartCoroutine(TypeText());
        }
        else
        {
            // --- โหมดไม่มี Dialogue (ขึ้น Tutorial + ปุ่มทันที) ---
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (tutorialImage != null) tutorialImage.SetActive(true);

            // ปลุกปุ่มให้ปรากฏขึ้นทันที
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("ลืมลากปุ่ม Next Button มาใส่ใน Inspector หรือเปล่าครับ?");
            }
        }
    }

    IEnumerator TypeText()
    {
        foreach (char letter in dialogueContent.ToCharArray())
        {
            contentText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
    }

    public void ChangeScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
