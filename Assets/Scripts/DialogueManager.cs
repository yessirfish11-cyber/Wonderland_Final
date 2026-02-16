using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public NPC currentTalkingNPC;

    [Header("Tutorial UI")]
    public GameObject tutorialPanel; // ลาก Tutorial Panel มาใส่ใน Inspector
    public Image tutorialImage;      // (Option) ถ้าต้องการเปลี่ยนรูปสอนตาม NPC

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    // ฟังก์ชันใหม่: แสดงหน้าวิธีเล่น
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);

            // ถ้าอยากให้รูปวิธีเล่นเปลี่ยนตาม NPC (ถ้าคุณเพิ่มตัวแปรใน NPC แล้ว)
            // if (currentTalkingNPC.tutorialSprite != null) 
            //    tutorialImage.sprite = currentTalkingNPC.tutorialSprite;
        }
        else
        {
            // ถ้าไม่มีหน้า Tutorial ให้โหลดซีนไปเลยกันเกมค้าง
            StartMiniGameFromCustomPanel();
        }
    }

    // ฟังก์ชันใหม่: ปุ่ม "เริ่มเกม" ในหน้า Tutorial จะมาเรียกใช้อันนี้
    public void StartMiniGameFromCustomPanel()
    {
        if (currentTalkingNPC != null)
        {
            SceneManager.LoadScene(currentTalkingNPC.nextSceneName);
        }
    }

    // สำหรับปุ่ม Skip
    public void OnSkipClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.SkipToLastLine();
    }

    // สำหรับปุ่ม Auto
    public void OnAutoClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.ToggleAutoPlay();
    }

    // สำหรับปุ่ม Continue (ปุ่มลูกศรที่ขึ้นตอนพิมพ์จบ)
    public void OnContinueClicked()
    {
        if (currentTalkingNPC != null) currentTalkingNPC.NextLine();
    }
}
