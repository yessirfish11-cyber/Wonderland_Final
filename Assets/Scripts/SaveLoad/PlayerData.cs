using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float x;
    public float y;
    public string saveTime;

    public PlayerData(float x, float y, string time)
    {
        this.x = x;
        this.y = y;
        this.saveTime = time;
    }
}