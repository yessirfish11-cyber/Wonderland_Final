using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string name;
    [TextArea(3, 10)]
    public string sentence;
}

public class NPC : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject interactPrompt;
    public GameObject contButton;

    [Header("Auto & Skip Settings")]
    public bool isAutoPlay = false;
    public float autoPlayDelay = 2.0f; // เวลาที่รอหลังจากพิมพ์จบก่อนไปบรรทัดถัดไป
    private Coroutine typingCoroutine;
    private Coroutine autoNextLineCoroutine;

    [Header("Settings")]
    public List<DialogueLine> dialogueLines;
    public float wordSpeed = 0.05f;
    public string nextSceneName; // ชื่อซีนที่จะไปคุยต่อ

    [Header("Flow Control")]
    public bool isInDialogueScene; // ติ๊กถูกถ้าสคริปต์นี้อยู่ในซีนที่สอง
    public GameObject tutorialPanel;

    private int index;
    private bool playerIsClose;
    private bool isTyping;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                StartDialogue();
            }
            else if (!isTyping)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        index = 0;
        dialoguePanel.SetActive(true);
        interactPrompt.SetActive(false);
        StartCoroutine(TypeSentence());
        DialogueManager.Instance.currentTalkingNPC = this;
    }

    IEnumerator TypeSentence()
    {
        dialogueText.text = "";
        contButton.SetActive(false);
        nameText.text = dialogueLines[index].name;

        foreach (char letter in dialogueLines[index].sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }

        // เมื่อพิมพ์จบประโยค
        contButton.SetActive(true);

        if (isAutoPlay)
        {
            if (autoNextLineCoroutine != null) StopCoroutine(autoNextLineCoroutine);
            autoNextLineCoroutine = StartCoroutine(AutoNextLineTimer());
        }
    }

    public void NextLine()
    {
        if (index < dialogueLines.Count - 1)
        {
            index++;
            StartCoroutine(TypeSentence());
        }
        else
        {
            FinishDialogue();
        }
    }

    public void FinishDialogue()
    {
        dialoguePanel.SetActive(false); // ปิดหน้าต่างคุย

        // ตรวจสอบว่า NPC ตัวนี้อยู่ในฉากที่ต้อง "เลือกของ" หรือเปล่า?
        // (ใช้ตัวแปร Is In Dialogue Scene ใน Inspector ที่คุณมีอยู่แล้ว)
        if (isInDialogueScene)
        {
            // กรณี Scene เลือกเรือ: ให้ Manager รอการกด E ที่กองไม้
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.currentTalkingNPC = this;
                DialogueManager.Instance.PrepareSelectionPhase();
            }
            Debug.Log("จบการสนทนา: เข้าสู่ช่วงเลือกเรือในฉากนี้");
        }
        else
        {
            // กรณี Scene แรก (GameScene): ให้โหลด Scene ถัดไปทันที
            Debug.Log("จบการสนทนา: กำลังเปลี่ยนไป Scene ถัดไป...");
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("ลืมใส่ชื่อ Next Scene Name ใน Inspector ของ NPC!");
            }
        }
    }

    // --- ส่วนของการตรวจจับระยะ (Trigger) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) { playerIsClose = true; interactPrompt.SetActive(true); }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) { playerIsClose = false; interactPrompt.SetActive(false); }
    }

    public void ToggleAutoPlay()
    {
        isAutoPlay = !isAutoPlay;

        // ถ้าเปิด Auto ตอนที่พิมพ์จบพอดี ให้เริ่มนับถอยหลังทันที
        if (isAutoPlay && dialogueText.text == dialogueLines[index].sentence)
        {
            if (autoNextLineCoroutine != null) StopCoroutine(autoNextLineCoroutine);
            autoNextLineCoroutine = StartCoroutine(AutoNextLineTimer());
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Skip
    public void SkipToLastLine()
    {
        StopAllCoroutines();
        index = dialogueLines.Count - 1; // ไปประโยคสุดท้าย
        dialogueText.text = dialogueLines[index].sentence;
        nameText.text = dialogueLines[index].name;
        contButton.SetActive(true);
        isAutoPlay = false; // ปิด Auto เมื่อกด Skip เพื่อความปลอดภัย
    }

    IEnumerator AutoNextLineTimer()
    {
        yield return new WaitForSeconds(autoPlayDelay);
        // ตรวจสอบว่ายังเปิด Auto อยู่และยังไม่จบการสนทนา
        if (isAutoPlay && dialoguePanel.activeInHierarchy)
        {
            NextLine();
        }
    }
}
