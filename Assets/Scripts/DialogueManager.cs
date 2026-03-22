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
        // เปิด DialogueBox เพื่อโชว์ผลลัพธ์
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultText != null) resultText.text = message;

            if (closeResultButton != null)
            {
                closeResultButton.gameObject.SetActive(true);
                closeResultButton.onClick.RemoveAllListeners();

                if (isCorrect)
                {
                    // ถ้าถูก: กดแล้วไปหน้า Tutorial
                    closeResultButton.onClick.AddListener(TransitionToTutorial);
                }
                else
                {
                    // ถ้าผิด: กดแล้วให้กลับไปเปิดหน้าเลือกเรือใหม่
                    closeResultButton.onClick.AddListener(BackToSelection);
                }
            }
        }
    }

    // ฟังก์ชันสำหรับกรณีตอบผิด: ปิด DialogueBox แล้วเปิดหน้าเลือกเรือใหม่
    void BackToSelection()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (boatSelectionPanel != null) boatSelectionPanel.SetActive(true);
        isWaitingForSelection = true; // มั่นใจว่ายังอยู่ในโหมดเลือก
    }

    void TransitionToTutorial()
    {
        isWaitingForSelection = false; // จบช่วงเลือกของจริง
        if (resultPanel != null) resultPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
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
        boatSelectionPanel.SetActive(false);

        string feedback = "";
        bool correct = false;

        // สมมติ Index 1 คือลำที่ถูก
        if (boatIndex == 1)
        {
            feedback = "ข้าว่าแล้วเจ้านี่ตาถึงเสียจริง";
            correct = true;
        }
        else
        {
            feedback = "ข้าก็พอดูออกละว่าเจ้ามันเบาปัญญาน่ะ เห้อ…";
            correct = false;
        }

        ShowResult(feedback, correct);
    }

}
