using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


[System.Serializable]
public class DialogueChoice
{
    public string choiceText;      // ข้อความบนปุ่ม
    public int targetIndex;        // ดัชนี (index) ของบทสนทนาที่จะไป
    public bool isCorrect;         // ตัวเลือกนี้ถูกหรือไม่ (ตามโจทย์: ถ้าผิดให้ย้อนกลับ)
}

[System.Serializable]
public class DialogueLine
{
    public string name;
    public Sprite characterImage;// ชื่อผู้พูด (Player หรือ NPC)
    [TextArea(3, 10)]
    public string sentence;  // ข้อความที่พูด

    [Header("Branching (Optional)")]
    public List<DialogueChoice> choices; // ถ้า List นี้ว่าง จะไม่มีปุ่มขึ้น

    [Header("Tutorial Settings")]
    public Sprite tutorialSprite; // ลากรูปภาพสอนเล่นของ NPC ตัวนี้มาใส่
    [TextArea(2, 5)]
    public string tutorialDescription; // เขียนคำอธิบายวิธีเล่น (ถ้ามี)
}

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    private int index;
    public GameObject interactPrompt;

    public TextMeshProUGUI nameText; // **เพิ่ม: ตัวหนังสือแสดงชื่อผู้พูด**
    public Image portraitImage;

    // เปลี่ยนจาก string[] เป็น List ของ DialogueLine
    public List<DialogueLine> dialogueLines;

    public GameObject contButton;
    public float wordSpeed;
    public bool playerIsClose;
    public string nextSceneName;

    [Header("Auto Play")]
    public bool isAutoPlay = false;       // สถานะ Auto
    public float autoPlayDelay = 2.0f;    // เวลาที่รอหลังจากพิมพ์จบ (วินาที)
    private Coroutine typingCoroutine;
    [Header("Custom Tutorial")]
    public GameObject customTutorialPanel;

    [Header("Choice System")]
    public GameObject choicePanel;      // Panel ที่เก็บปุ่ม Choice
    public Button[] choiceButtons;      // ลากปุ่ม Choice มาใส่ (เช่น 2 ปุ่ม)
    public TextMeshProUGUI[] choiceTexts; // Text ของปุ่ม Choice

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                StartDialogue();
            }
            // ตรวจสอบว่าพิมพ์จบประโยคหรือยัง ถ้าจบแล้วให้ไปบรรทัดถัดไป
            else if (dialogueText.text == dialogueLines[index].sentence)
            {
                NextLine();
            }
            // (Option) ถ้ายังพิมพ์ไม่จบแต่กด E ให้แสดงข้อความเต็มทันที
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[index].sentence;
            }
        }

        // ปรับปรุงเงื่อนไขบรรทัดที่ 54 ให้ปลอดภัยขึ้น
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy && index < dialogueLines.Count)
        {
            if (dialogueText.text == dialogueLines[index].sentence)
            {
                if (contButton != null) contButton.SetActive(true);
            }
        }
    }

    public void StartDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.currentTalkingNPC = this;
        }

        index = 0;
        dialoguePanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Typing());
    }

    public void ToggleAutoPlay()
    {
        isAutoPlay = !isAutoPlay;

        // ถ้าพิมพ์จบอยู่แล้วและเพิ่งกดเปิด Auto ให้ข้ามไปอันถัดไปเลย
        if (isAutoPlay && dialoguePanel.activeInHierarchy && dialogueText.text == dialogueLines[index].sentence)
        {
            StartCoroutine(AutoNextLineTimer());
        }
    }

    public void SkipToLastLine()
    {
        if (dialogueLines != null && dialogueLines.Count > 0)
        {
            StopAllCoroutines();
            index = dialogueLines.Count - 1; // ไปที่บรรทัดสุดท้าย

            // แสดงข้อความสุดท้ายทันที
            dialogueText.text = dialogueLines[index].sentence;
            nameText.text = dialogueLines[index].name;

            // หลังจากโชว์ประโยคสุดท้ายแล้ว เมื่อกดปุ่มต่อหรือ Skip อีกครั้ง 
            // มันจะไปเรียก FinishDialogue() ซึ่งใช้ nextSceneName ของตัวมันเอง
        }
    }

    public void zeroText()
    {
        // 1. เช็คว่า dialogueText ยังอยู่ไหมก่อนล้างค่า
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        index = 0;

        // 2. เช็คว่า dialoguePanel ยังอยู่ไหมก่อนสั่งปิด (จุดที่เกิด Error บ่อยที่สุด)
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 3. เช็ค interactPrompt ด้วยเพื่อความปลอดภัย
        if (playerIsClose && interactPrompt != null)
        {
            interactPrompt.SetActive(true);
        }
    }

    IEnumerator Typing()
    {
        dialogueText.text = "";
        contButton.SetActive(false);
        choicePanel.SetActive(false); // ปิดปุ่มไว้ก่อน

        if (dialogueLines[index] != null)
        {
            nameText.text = dialogueLines[index].name;
            if (portraitImage != null) portraitImage.sprite = dialogueLines[index].characterImage;

            foreach (char letter in dialogueLines[index].sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }

            // --- ส่วนที่เพิ่มใหม่: เช็กว่าบรรทัดนี้มีตัวเลือกหรือไม่ ---
            if (dialogueLines[index].choices != null && dialogueLines[index].choices.Count > 0)
            {
                ShowChoices();
            }
            else
            {
                if (isAutoPlay) StartCoroutine(AutoNextLineTimer());
            }
        }
    }

    void ShowChoices()
    {
        choicePanel.SetActive(true);
        contButton.SetActive(false); // ซ่อนปุ่ม Next ทั่วไป

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < dialogueLines[index].choices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = dialogueLines[index].choices[i].choiceText;

                int choiceIdx = i; // ป้องกันปัญหา Closure ใน Listener
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIdx));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        DialogueChoice selected = dialogueLines[index].choices[choiceIndex];

        if (selected.isCorrect)
        {
            // ถ้าเลือกถูก ไปยัง Index ที่ระบุ
            index = selected.targetIndex;
        }
        else
        {
            // ถ้าเลือกผิด ย้อนกลับไป 1 บทสนทนา (ตามโจทย์)
            // ระวังไม่ให้ index ติดลบ
            index = Mathf.Max(0, index - 1);
        }

        choicePanel.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(Typing());
    }


    IEnumerator AutoNextLineTimer()
    {
        yield return new WaitForSeconds(autoPlayDelay);

        // ตรวจสอบอีกครั้งว่ายังเปิด Auto อยู่และยังอยู่ใน Panel เดิม
        if (isAutoPlay && dialoguePanel.activeInHierarchy && dialogueText.text == dialogueLines[index].sentence)
        {
            NextLine();
        }
    }

    public void NextLine()
    {

        contButton.SetActive(false);

        if (index < dialogueLines.Count - 1)
        {
            index++; // ขยับไปข้อความถัดไป
            StopAllCoroutines(); // หยุดการพิมพ์เก่าก่อนเริ่มอันใหม่
            StartCoroutine(Typing());
        }
        else
        {
            // ถ้าหมดแล้วให้จบการสนทนา
            FinishDialogue();
        }
    }

    public void FinishDialogue()
    {
        Debug.Log("<color=yellow>จบการสนทนา เปิด Tutorial เฉพาะตัว</color>");

        zeroText(); // ปิดหน้าต่างคุย

        if (customTutorialPanel != null)
        {
            customTutorialPanel.SetActive(true); // เปิดหน้าวิธีเล่นของตัวเอง
        }
        else
        {
            // ถ้าไม่มีหน้าพิเศษ ให้ลองไปใช้ตัวกลางใน Manager (ถ้ามี) หรือโหลดซีนเลย
            SceneManager.LoadScene(nextSceneName);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            if (!dialoguePanel.activeInHierarchy)
                interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            interactPrompt.SetActive(false);
            zeroText();
        }
    }
}
