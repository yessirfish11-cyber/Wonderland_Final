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
    public Sprite characterImage;// ชื่อผู้พูด (Player หรือ NPC)
    [TextArea(3, 10)]
    public string sentence;  // ข้อความที่พูด
}

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogue;
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
        if (dialoguePanel.activeInHierarchy && index < dialogueLines.Count)
        {
            if (dialogueText.text == dialogueLines[index].sentence)
            {
                contButton.SetActive(true);
            }
        }
    }

    public void StartDialogue()
    {
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
        // 1. ตรวจสอบว่ามีข้อมูลบทสนทนาอยู่จริง
        if (dialogueLines != null && dialogueLines.Count > 0)
        {
            // 2. หยุดการพิมพ์และ Timer ทุกอย่างที่กำลังทำงาน
            StopAllCoroutines();

            // 3. กระโดดไปที่ประโยคสุดท้าย (ลำดับสุดท้ายของ List)
            index = dialogueLines.Count - 1;

            // 4. แสดงผลข้อความสุดท้ายทันที (ใช้ Typing เพื่อเปลี่ยนภาพและชื่อด้วย)
            StartCoroutine(Typing());

            // 5. ปิดโหมด Auto (เพื่อไม่ให้มันข้ามจบไวเกินไป)
            isAutoPlay = false;

            Debug.Log("ข้ามไปประโยคสุดท้ายแล้ว!");
        }
    }

    public void zeroText()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);

        if (playerIsClose)
            interactPrompt.SetActive(true);
    }

    IEnumerator Typing()
    {
        dialogueText.text = "";
        contButton.SetActive(false);
        // Debug เช็กว่าใน List มีข้อความจริงไหม
        if (dialogueLines[index] != null)
        {
            Debug.Log("กำลังจะพิมพ์ข้อความ: " + dialogueLines[index].sentence);
            nameText.text = dialogueLines[index].name;

            if (portraitImage != null)
                portraitImage.sprite = dialogueLines[index].characterImage;

            foreach (char letter in dialogueLines[index].sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
        }
        else
        {
            Debug.LogError("Error: ข้อมูลใน DialogueLines ลำดับที่ " + index + " ว่างเปล่า!");
        }

        if (isAutoPlay)
        {
            StartCoroutine(AutoNextLineTimer());
        }
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
        Debug.Log("Trying load scene: " + nextSceneName);
        zeroText();
        SceneManager.LoadScene(nextSceneName);
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
