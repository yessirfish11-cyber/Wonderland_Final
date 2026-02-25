using UnityEngine;

/// <summary>
/// จัดการระบบเก็บไอเทม - เก็บครบ 3 แล้วสุ่มเกิด Final Object
/// </summary>
public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [Header("Collection Settings")]
    [Tooltip("จำนวนไอเทมที่ต้องเก็บก่อนเกิด Final Object")]
    public int itemsNeededForFinal = 3;

    [Header("Final Object")]
    [Tooltip("Object ที่จะเกิดหลังเก็บครบ (Prefab)")]
    public GameObject finalObjectPrefab;
    
    [Tooltip("จุดที่จะสุ่มให้ Final Object เกิด")]
    public Transform[] spawnPoints;
    
    [Tooltip("ข้อความแจ้งเตือน (Optional)")]
    public string finalObjectMessage = "🎯 Final item appeared!";

    // State
    private int itemsCollected = 0;
    private bool finalObjectSpawned = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // เช็คว่าตั้งค่าครบหรือไม่
        if (finalObjectPrefab == null)
            Debug.LogError("[CollectionManager] ไม่ได้ใส่ Final Object Prefab!");

        if (spawnPoints.Length == 0)
            Debug.LogError("[CollectionManager] ไม่มี Spawn Points!");
    }

    /// <summary>
    /// เรียกจาก Player เมื่อเก็บไอเทมปกติ
    /// </summary>
    public void OnItemCollected()
    {
        itemsCollected++;
        Debug.Log($"[CollectionManager] Items: {itemsCollected}/{itemsNeededForFinal}");

        if (itemsCollected >= itemsNeededForFinal && !finalObjectSpawned)
        {
            SpawnFinalObject();
        }
    }

    void SpawnFinalObject()
    {
        if (finalObjectPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[CollectionManager] ไม่สามารถ spawn Final Object ได้!");
            return;
        }

        finalObjectSpawned = true;

        // สุ่มจุด spawn
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // สร้าง Final Object
        GameObject finalObj = Instantiate(
            finalObjectPrefab, 
            spawnPoint.position, 
            Quaternion.identity
        );

        Debug.Log($"[CollectionManager] ✨ {finalObjectMessage}");
        Debug.Log($"[CollectionManager] Spawned at: {spawnPoint.name}");

        // แจ้งเตือน Player (Optional)
        ShowNotification();
    }

    void ShowNotification()
    {
        // TODO: แสดง UI แจ้งเตือน (ถ้าต้องการ)
        // เช่น: UIManager4.Instance?.ShowNotification(finalObjectMessage);
    }

    /// <summary>
    /// เช็คว่าเก็บครบหรือยัง
    /// </summary>
    public bool HasCollectedEnough()
    {
        return itemsCollected >= itemsNeededForFinal;
    }

    /// <summary>
    /// เช็คว่า Final Object เกิดแล้วหรือยัง
    /// </summary>
    public bool IsFinalObjectSpawned()
    {
        return finalObjectSpawned;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        // แสดงจุด spawn
        Gizmos.color = Color.yellow;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 1f);
            }
        }
    }
}