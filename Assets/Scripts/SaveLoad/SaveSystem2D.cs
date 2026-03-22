using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class SaveSystem2D
{
    public static int selectedSlot = -1;

    private static string GetPath(int slot) => Application.persistentDataPath + "/save_" + slot + ".json";

    public static void Save(int slot, Vector2 pos)
    {
        string currentTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string currentScene = SceneManager.GetActiveScene().name; // ดึงชื่อซีนปัจจุบัน

        PlayerData data = new PlayerData(pos.x, pos.y, currentTime, currentScene);
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data));
        Debug.Log("Saved to Scene: " + currentScene);
    }

    public static PlayerData Load(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
        }
        return null;
    }

    public static void DeleteSave(int slot)
    {
        string path = Application.persistentDataPath + "/save_" + slot + ".json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"ลบไฟล์ใน Slot {slot} เรียบร้อย!");
        }
    }
}
