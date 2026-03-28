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
    public enum AfterDialogueAction { ChangeScene, TriggerSelection, JustStay, UnlockTransition }

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

    [Header("Player Reference")]
    public GameObject player;
    private Animator playerAnim;// ลากตัวละคร Player มาใส่ใน Inspector ของ NPC ทุกตัว

    [Header("Settings")]
    public List<DialogueLine> dialogueLines;
    public float wordSpeed = 0.05f;
    public string nextSceneName; // ชื่อซีนที่จะไปคุยต่อ

    [Header("Behavior Settings")]
    public AfterDialogueAction actionAfterFinish;

    [Header("External Trigger")]
    public SceneTransitionTrigger targetTrigger;

    [Header("Flow Control")]
    public bool isInDialogueScene; // ติ๊กถูกถ้าสคริปต์นี้อยู่ในซีนที่สอง
    public GameObject tutorialPanel;

    private int index;
    private bool playerIsClose;
    private bool isTyping;
    private bool hasFinishedDialogue;

    void Update()
    {
        // กด E เพื่อเริ่มคุย หรือกดเพื่อไปประโยคถัดไป
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
        TogglePlayerControl(false); // หยุดตัวละคร

        index = 0;
        dialoguePanel.SetActive(true);
        interactPrompt.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence());

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.currentTalkingNPC = this;
    }

    IEnumerator TypeSentence()
    {
        isTyping = true;
        dialogueText.text = "";
        contButton.SetActive(false);
        nameText.text = dialogueLines[index].name;

        foreach (char letter in dialogueLines[index].sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }

        isTyping = false;
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
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence());
        }
        else
        {
            FinishDialogue();
        }
    }

    public void FinishDialogue()
    {
        dialoguePanel.SetActive(false);
        TogglePlayerControl(true); // ให้ผู้เล่นเดินได้ปกติ

        // แยก Logic ตาม Action ที่เลือกใน Inspector
        switch (actionAfterFinish)
        {
            case AfterDialogueAction.UnlockTransition:
                // คุยจบแล้ว "ปลดล็อก" จุดวาร์ปที่อยู่ห่างออกไป (ผู้เล่นต้องเดินไปชนเอง)
                if (targetTrigger != null)
                {
                    targetTrigger.UnlockTrigger();
                    Debug.Log("UnlockTransition: จุดวาร์ปเปิดแล้ว เดินไปชนได้เลย");
                }
                break;

            case AfterDialogueAction.ChangeScene:
                // คุยจบแล้ว "วาร์ปทันที" (ไม่ต้องเดินไปไหน)
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                break;

            case AfterDialogueAction.TriggerSelection:
                // คุยจบแล้ว "เปิดระบบเลือกช้อยส์/เลือกเรือ"
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.currentTalkingNPC = this;
                    DialogueManager.Instance.PrepareSelectionPhase();
                }
                break;

            case AfterDialogueAction.JustStay:
                // คุยจบแล้ว "ไม่มีอะไรเกิดขึ้น" (คุยเล่นปกติ)
                break;
        }
    }

    private void TogglePlayerControl(bool canMove)
    {
        if (player != null)
        {
            PlayerCtrl playerScript = player.GetComponent<PlayerCtrl>();
            Animator playerAnim = player.GetComponent<Animator>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (playerScript != null)
            {
                if (!canMove && rb != null) rb.linearVelocity = Vector2.zero; // หยุดตัวละคร
                playerScript.enabled = canMove;
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
