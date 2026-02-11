using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapName = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if(File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = 
                GameObject.Find(saveData.mapName).GetComponent<PolygonCollider2D>();
        }
        else
        {
            SaveGame();
        }
    }
}
