using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameControl : MonoBehaviour
{
    public GameObject player;      // ลากตัวละครมาใส่ (เฉพาะใน GameScene)
    public GameObject menuPanel;   // ลากหน้าต่าง UI มาใส่
    // สร้าง Array เพื่อลาก Text ของแต่ละ Slot มาใส่ (เช่น Slot1_Text, Slot2_Text)
    public TMP_Text[] slotTexts;
    public GameObject[] deleteButtons; // ลากปุ่มลบของแต่ละ Slot มาใส่ที่นี่

    void Start()
    {
        if (SaveSystem2D.selectedSlot != -1 && player != null)
        {
            LoadFromSlot(SaveSystem2D.selectedSlot);
            SaveSystem2D.selectedSlot = -1;
        }
        // อัปเดตตัวหนังสือบนปุ่มทันทีที่เริ่ม (ถ้าอยู่ในหน้า MainMenu)
        UpdateSlotLabels();
    }

    public void UpdateSlotLabels()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            PlayerData data = SaveSystem2D.Load(i + 1);
            if (data != null)
            {
                slotTexts[i].text = "Save " + (i + 1) + "\n(" + data.saveTime + ")";
                if (deleteButtons.Length > i) deleteButtons[i].SetActive(true); // มีไฟล์ = โชว์ปุ่มลบ
            }
            else
            {
                slotTexts[i].text = "Save " + (i + 1) + "\n(Empty)";
                if (deleteButtons.Length > i) deleteButtons[i].SetActive(false); // ไม่มีไฟล์ = ซ่อนปุ่มลบ
            }
        }
    }

    // --- ฟังก์ชันสำหรับปุ่มใน Main Menu ---
    public void MainMenu_LoadSlot(int slot)
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/save_" + slot + ".json"))
        {
            SaveSystem2D.selectedSlot = slot;
            // ใส่ชื่อ Scene เกมของคุณตรงนี้
            SceneManager.LoadScene("GameScene");
        }
    }

    // --- ฟังก์ชันสำหรับปุ่มใน Game Scene ---
    public void Game_SaveSlot(int slot)
    {
        SaveSystem2D.Save(slot, player.transform.position);
        UpdateSlotLabels();
        menuPanel.SetActive(false);
    }

    public void LoadFromSlot(int slot)
    {
        PlayerData data = SaveSystem2D.Load(slot);
        if (data != null && player != null)
        {
            player.transform.position = new Vector2(data.x, data.y);
        }
    }

    public void ClickDeleteSlot(int slot)
    {
        SaveSystem2D.DeleteSave(slot);
        UpdateSlotLabels(); // ลบแล้วต้องอัปเดตชื่อปุ่มให้เป็น (Empty) ทันที
    }
}
