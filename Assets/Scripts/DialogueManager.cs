using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public NPC currentTalkingNPC;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button closeResultButton;

    [Header("Selection UI")]
    public GameObject boatSelectionPanel;

    [Header("Tutorial UI")]
    public GameObject tutorialPanel;

    public bool isWaitingForSelection = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        resultPanel.SetActive(false);
        tutorialPanel.SetActive(false);
    }

    public void PrepareSelectionPhase()
    {
        isWaitingForSelection = true;
        Debug.Log("คุยจบแล้ว กรุณาไปเลือกวัตถุในฉาก");
    }

    public void ShowResult(string message, bool isCorrect)
    {
        if (currentTalkingNPC == null) return;

        isWaitingForSelection = false; // ปิดโหมดรอเลือก (เลือกได้ครั้งเดียว)

        // เปิด DialogueBox เดิมของ NPC ขึ้นมาใหม่
        currentTalkingNPC.dialoguePanel.SetActive(true);
        currentTalkingNPC.nameText.text = isCorrect ? "ระบบ: ถูกต้อง" : "ระบบ: ไม่ถูกต้อง";

        // แสดงข้อความ Feedback
        currentTalkingNPC.StopAllCoroutines();
        currentTalkingNPC.dialogueText.text = message;

        // เปลี่ยนปุ่ม Continue ให้ไปเปิด Tutorial แทน
        currentTalkingNPC.contButton.SetActive(true);
        Button btn = currentTalkingNPC.contButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(TransitionToTutorial);
    }

    void TransitionToTutorial()
    {
        currentTalkingNPC.dialoguePanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }

    void ShowTutorial()
    {
        resultPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    // เรียกใช้จากปุ่มในหน้า Tutorial
    public void StartGame()
    {
        if (currentTalkingNPC != null)
        {
            SceneManager.LoadScene(currentTalkingNPC.nextSceneName);
        }
    }

    public void OnAutoClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.ToggleAutoPlay();
    }

    public void OnSkipClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.SkipToLastLine();
    }

    // สำหรับปุ่ม Continue (ลูกศร)
    public void OnContinueClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.NextLine();
    }

    // ฟังก์ชันนี้จะผูกกับปุ่มเรือ 3 ปุ่ม
    public void SelectBoat(int boatIndex)
    {
        boatSelectionPanel.SetActive(false); // ปิดหน้าเลือกเรือทันทีที่กด

        string feedback = "";
        bool correct = false;

        // สมมติว่าลำที่ 2 (Index 1) คือลำที่ถูก
        if (boatIndex == 1)
        {
            feedback = "ถูกต้อง! เรือลำนี้แข็งแรงที่สุด เหมาะกับการเดินทาง";
            correct = true;
        }
        else
        {
            feedback = "แย่แล้ว! เรือลำนี้ดูเหมือนจะมีรอยรั่วนะ";
            correct = false;
        }

        ShowResult(feedback, correct); // เรียกใช้ ShowResult เดิมที่เราเขียนไว้
    }

}
