using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GameControl : MonoBehaviour
{
    public GameObject player;      // ลากตัวละครมาใส่ (เฉพาะใน GameScene)

    [Header("UI Panels")]
    public GameObject savePanel; // ลากหน้าต่าง Save มาใส่
    public GameObject loadPanel; // ลากหน้าต่าง Load มาใส่

    [Header("Save UI Settings")]
    public TMP_Text[] saveSlotTexts;       // ลาก Text 1,2,3 ของหน้า Save มาใส่
    public GameObject[] saveDeleteButtons; // ลากปุ่มลบ 1,2,3 ของหน้า Save มาใส่

    [Header("Load UI Settings")]
    public TMP_Text[] loadSlotTexts;       // ลาก Text 1,2,3 ของหน้า Load มาใส่
    public GameObject[] loadDeleteButtons; // ลากปุ่มลบ 1,2,3 ของหน้า Load มาใส่

    void Start()
    {
        // แก้ไขจุดที่ 1: ตรวจสอบ Slot ที่เลือกมาจากหน้าอื่น
        if (SaveSystem2D.selectedSlot != -1)
        {
            PlayerData data = SaveSystem2D.Load(SaveSystem2D.selectedSlot);

            // เช็คว่าเราอยู่ใน Scene ที่ถูกต้องตามไฟล์เซฟหรือยัง
            if (data != null && SceneManager.GetActiveScene().name == data.sceneName)
            {
                if (player != null)
                {
                    player.transform.position = new Vector2(data.x, data.y);
                }
                SaveSystem2D.selectedSlot = -1; // โหลดเสร็จแล้วรีเซ็ต
            }
        }

        UpdateSlotLabels();
    }

    public void UpdateSlotLabels()
    {
        // อัปเดตฝั่งหน้า Save
        UpdateSpecificUI(saveSlotTexts, saveDeleteButtons);

        // อัปเดตฝั่งหน้า Load
        UpdateSpecificUI(loadSlotTexts, loadDeleteButtons);
    }

    private void UpdateSpecificUI(TMP_Text[] texts, GameObject[] buttons)
    {
        if (texts == null) return;

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] == null) continue;

            // i + 1 จะได้เลข Slot 1, 2, 3 เสมอ ไม่ว่าจะเป็นหน้าไหน
            PlayerData data = SaveSystem2D.Load(i + 1);

            if (data != null)
            {
                texts[i].text = "Save " + (i + 1) + "\n(" + data.saveTime + ")";
                if (buttons.Length > i && buttons[i] != null) buttons[i].SetActive(true);
            }
            else
            {
                texts[i].text = "Save " + (i + 1) + "\n(Empty)";
                if (buttons.Length > i && buttons[i] != null) buttons[i].SetActive(false);
            }
        }
    }

    // --- ฟังก์ชันสำหรับปุ่มใน Main Menu ---
    public void MainMenu_LoadSlot(int slot)
    {
        PlayerData data = SaveSystem2D.Load(slot);
        if (data != null)
        {
            SaveSystem2D.selectedSlot = slot;
            // แก้ไขจุดที่ 2: โหลด Scene ตามที่บันทึกไว้ในข้อมูล
            SceneManager.LoadScene(data.sceneName);
        }
    }

    public void OpenSaveMenu()
    {
        savePanel.SetActive(true);
        if (loadPanel != null) loadPanel.SetActive(false); // ปิดหน้าต่างโหลดถ้าเปิดค้างไว้
        UpdateSlotLabels(); // อัปเดตวันที่ทันทีที่เปิด
    }

    public void OpenLoadMenu()
    {
        loadPanel.SetActive(true);
        if (savePanel != null) savePanel.SetActive(false); // ปิดหน้าต่างเซฟถ้าเปิดค้างไว้
        UpdateSlotLabels(); // อัปเดตวันที่ทันทีที่เปิด
    }

    // --- ฟังก์ชันสำหรับปุ่มใน Game Scene ---
    public void Game_SaveSlot(int slot)
    {
        if (player == null) return; // ถ้าไม่มีตัวละครในซีนนี้ ห้ามเซฟ

        SaveSystem2D.Save(slot, player.transform.position);
        UpdateSlotLabels();
    }

    public void Game_LoadSlot(int slot)
    {
        PlayerData data = SaveSystem2D.Load(slot);
        if (data != null)
        {
            // แก้ไขจุดที่ 3: เช็คว่าไฟล์ที่โหลดอยู่ซีนเดียวกับปัจจุบันไหม
            if (data.sceneName == SceneManager.GetActiveScene().name)
            {
                // ถ้าซีนเดียวกัน แค่วาร์ปตัวละคร
                if (player != null) player.transform.position = new Vector2(data.x, data.y);
                if (loadPanel != null) loadPanel.SetActive(false);
            }
            else
            {
                // ถ้าคนละซีน ให้ส่ง Slot ไปรอแล้วโหลดซีนใหม่
                SaveSystem2D.selectedSlot = slot;
                SceneManager.LoadScene(data.sceneName);
            }
        }
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
