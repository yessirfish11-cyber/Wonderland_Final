using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        LoadGame();
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // ค้นหา Confiner แบบปลอดภัย
        var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        string mName = "DefaultMap"; // ใส่ชื่อเริ่มต้นไว้ก่อน

        // เช็คทีละขั้นว่ามีของไหม
        if (confiner != null && confiner.BoundingShape2D != null)
        {
            mName = confiner.BoundingShape2D.gameObject.name;
        }

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            mapName = mName
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Save สำเร็จแล้ว และไม่พังแน่นอน!");
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            // เริ่มต้นกระบวนการโหลดฉาก
            StartCoroutine(LoadLevelRoutine(saveData));
        }
    }

    private IEnumerator LoadLevelRoutine(SaveData data)
    {
        // 1. โหลด Scene ตามที่บันทึกไว้
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);

        // รอจนกว่า Scene จะโหลดเสร็จ
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 2. หลังจากฉากโหลดเสร็จแล้ว ค่อยจัดการตัวละครและกล้อง
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
        }

        // 3. เซ็ตอัพ Cinemachine Confiner
        GameObject mapObject = GameObject.Find(data.mapName);
        if (mapObject != null)
        {
            var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.BoundingShape2D = mapObject.GetComponent<PolygonCollider2D>();
            }
        }
    }
}
