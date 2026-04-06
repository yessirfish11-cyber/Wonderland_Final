using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// จัดการ Spawn CollectibleItem แบบสุ่มในพื้นที่ (Area) ที่กำหนด
/// 
/// วิธีใช้:
/// 1. สร้าง GameObject ว่างๆ ชื่อ "ItemSpawnManager" ใน Scene
/// 2. แนบ Script นี้เข้าไป
/// 3. กำหนด spawnAreaCenter + spawnAreaSize (หรือเปิด useColliderBounds แล้วแนบ Collider2D)
/// 4. ลาก Prefab ของ CollectibleItem ใส่ช่อง itemPrefabs
/// 5. กด Play — Manager จะ Spawn อัตโนมัติตอน Start
/// </summary>
public class ItemSpawnManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector Settings
    // ─────────────────────────────────────────────

    [Header("Item Prefabs")]
    [Tooltip("Prefab ของ CollectibleItem ที่ต้องการ Spawn (สุ่มเลือก 1 ตัวต่อการ Spawn)")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Spawn Area")]
    [Tooltip("เปิดเพื่อใช้ขอบเขตจาก Collider2D บน GameObject นี้แทนการตั้งค่าด้วยมือ")]
    [SerializeField] private bool useColliderBounds = false;

    [Tooltip("จุดกึ่งกลางของพื้นที่ Spawn (World Space) — ใช้เมื่อ useColliderBounds = false")]
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;

    [Tooltip("ขนาดพื้นที่ Spawn (กว้าง × สูง) — ใช้เมื่อ useColliderBounds = false")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 10f);

    [Header("Spawn Settings")]
    [Tooltip("จำนวน Item ที่ Spawn ตอนเริ่มเกม")]
    [SerializeField] private int spawnCount = 5;

    [Tooltip("ระยะห่างขั้นต่ำระหว่าง Item แต่ละตัว (0 = ไม่สนใจ)")]
    [SerializeField] private float minDistanceBetweenItems = 1f;

    [Tooltip("จำนวนครั้งสูงสุดที่พยายามหาจุด Spawn ที่ว่าง ก่อนยอมแพ้")]
    [SerializeField] private int maxPlacementAttempts = 30;

    [Tooltip("Layer ที่ถือว่าเป็นสิ่งกีดขวาง (ถ้าไม่ต้องการเช็คให้ปล่อยว่าง)")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("รัศมีเช็ค Overlap กับสิ่งกีดขวาง")]
    [SerializeField] private float obstacleCheckRadius = 0.3f;

    [Header("Runtime Spawn")]
    [Tooltip("เปิดเพื่อให้ Spawn ใหม่อัตโนมัติเมื่อ Item ถูกเก็บครบ")]
    [SerializeField] private bool respawnWhenEmpty = false;

    [Tooltip("หน่วงเวลา (วินาที) ก่อน Respawn รอบใหม่")]
    [SerializeField] private float respawnDelay = 3f;

    // ─────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────

    private Bounds spawnBounds;
    private List<GameObject> activeItems = new List<GameObject>();
    private bool isWaitingRespawn = false;

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        BuildBounds();
    }

    private void Start()
    {
        SpawnAll();
    }

    private void Update()
    {
        if (!respawnWhenEmpty || isWaitingRespawn) return;

        // ลบ reference ที่ Destroy ไปแล้ว
        activeItems.RemoveAll(item => item == null);

        if (activeItems.Count == 0)
        {
            isWaitingRespawn = true;
            Invoke(nameof(RespawnAll), respawnDelay);
        }
    }

    // ─────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────

    /// <summary>Spawn Item ทั้งหมดตามจำนวนที่กำหนด</summary>
    public void SpawnAll()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("[ItemSpawnManager] ไม่มี itemPrefabs กำหนดไว้!");
            return;
        }

        activeItems.Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
        }
    }

    /// <summary>Spawn Item 1 ตัวในตำแหน่งสุ่ม</summary>
    public GameObject SpawnOne()
    {
        Vector2 pos;
        if (!TryGetValidPosition(out pos))
        {
            Debug.LogWarning("[ItemSpawnManager] หาจุด Spawn ที่ว่างไม่ได้ใน " + maxPlacementAttempts + " ครั้ง");
            return null;
        }

        GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        GameObject spawned = Instantiate(prefab, pos, Quaternion.identity);
        activeItems.Add(spawned);
        return spawned;
    }

    // ─────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────

    private void RespawnAll()
    {
        isWaitingRespawn = false;
        SpawnAll();
    }

    private void BuildBounds()
    {
        if (useColliderBounds)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                spawnBounds = col.bounds;
                return;
            }
            Debug.LogWarning("[ItemSpawnManager] useColliderBounds = true แต่ไม่มี Collider2D บน GameObject นี้ — ใช้ค่า Manual แทน");
        }

        spawnBounds = new Bounds(spawnAreaCenter, spawnAreaSize);
    }

    private bool TryGetValidPosition(out Vector2 result)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                Random.Range(spawnBounds.min.y, spawnBounds.max.y)
            );

            // เช็คสิ่งกีดขวาง
            if (obstacleLayer != 0)
            {
                if (Physics2D.OverlapCircle(candidate, obstacleCheckRadius, obstacleLayer))
                    continue;
            }

            // เช็คระยะห่างจาก Item อื่น
            if (minDistanceBetweenItems > 0f && IsTooClose(candidate))
                continue;

            result = candidate;
            return true;
        }

        result = Vector2.zero;
        return false;
    }

    private bool IsTooClose(Vector2 candidate)
    {
        foreach (var item in activeItems)
        {
            if (item == null) continue;
            if (Vector2.Distance(candidate, item.transform.position) < minDistanceBetweenItems)
                return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────
    // Gizmos — แสดง Spawn Area ใน Scene View
    // ─────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        // สร้าง bounds ชั่วคราวเพื่อแสดงตอน Edit Mode
        Bounds preview;
        if (useColliderBounds)
        {
            Collider2D col = GetComponent<Collider2D>();
            preview = col != null ? col.bounds : new Bounds(spawnAreaCenter, spawnAreaSize);
        }
        else
        {
            preview = new Bounds(spawnAreaCenter, spawnAreaSize);
        }

        // กรอบพื้นที่ Spawn (เหลือง)
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.25f);
        Gizmos.DrawCube(preview.center, preview.size);

        Gizmos.color = new Color(1f, 0.9f, 0f, 0.8f);
        Gizmos.DrawWireCube(preview.center, preview.size);

        // วงกลมระยะห่างขั้นต่ำ (ฟ้า) — แสดงรอบจุดกึ่งกลางเป็นตัวอย่าง
        if (minDistanceBetweenItems > 0f)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(preview.center, minDistanceBetweenItems);
        }
    }
}