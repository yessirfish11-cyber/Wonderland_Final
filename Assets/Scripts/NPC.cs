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

    [Header("Player Reference")]
    public GameObject player;
    private Animator playerAnim;// ลากตัวละคร Player มาใส่ใน Inspector ของ NPC ทุกตัว

    [Header("Settings")]
    public List<DialogueLine> dialogueLines;
    public float wordSpeed = 0.05f;
    public string nextSceneName; // ชื่อซีนที่จะไปคุยต่อ

    public enum AfterDialogueAction { ChangeScene, TriggerSelection, JustStay }

    [Header("Behavior Settings")]
    public AfterDialogueAction actionAfterFinish;

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
        // 1. ดึง Component จาก Player มาเก็บไว้ก่อน
        if (player != null)
        {
            PlayerCtrl playerScript = player.GetComponent<PlayerCtrl>();
            playerAnim = player.GetComponent<Animator>(); // ดึง Animator ของตัวละคร
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (playerScript != null && playerAnim != null)
            {
                // หยุดตัวละครไม่ให้ไถล
                if (rb != null) rb.linearVelocity = Vector2.zero;

                // ดึงทิศทางล่าสุด (ต้องไปแก้ lastDirectionState เป็น public ใน PlayerCtrl ก่อนนะ)
                int lastDir = playerScript.lastDirectionState;

                // ส่งค่า Idle ไปที่ Animator (เช่น 10, 20, 30, 40)
                playerAnim.SetInteger("State", lastDir * 10);

                // ปิดสคริปต์เดิน
                playerScript.enabled = false;
            }
        }

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

        // 3. เปิดสคริปต์เดินคืนให้ Player
        if (player != null)
        {
            PlayerCtrl playerScript = player.GetComponent<PlayerCtrl>();
            if (playerScript != null) playerScript.enabled = true;
        }

        // ตรวจสอบเงื่อนไขตามที่เราเลือกไว้ใน Inspector
        switch (actionAfterFinish)
        {
            case AfterDialogueAction.ChangeScene:
                Debug.Log("จบการสนทนา: เปลี่ยนซีน...");
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                break;

            case AfterDialogueAction.TriggerSelection:
                Debug.Log("จบการสนทนา: เข้าสู่ช่วงเลือกของ");
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.currentTalkingNPC = this;
                    DialogueManager.Instance.PrepareSelectionPhase();
                }
                break;

            case AfterDialogueAction.JustStay:
                Debug.Log("จบการสนทนา: อยู่ซีนเดิม (คุยเล่นปกติ)");
                // ไม่ต้องทำอะไร แค่ปิด Dialogue Panel (ซึ่งทำไปแล้วบรรทัดแรก)
                // ผู้เล่นจะสามารถเดินต่อได้ทันทีในซีนเดิม
                break;
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
