using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float x;
    public float y;
    public string saveTime;
    public string sceneName;

    public PlayerData(float x, float y, string time, string scene)
    {
        this.x = x;
        this.y = y;
        this.saveTime = time;
        this.sceneName = scene;
    }
}