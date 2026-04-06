using UnityEngine;

/// <summary>
/// แนบกับ GameObject ที่มี Collider2D (เปิด Is Trigger)
/// เมื่อผู้เล่นเดินเข้าชน → เรียก GameManager แสดง WinPanel
///
/// วิธีใช้:
/// 1. สร้าง GameObject แนบ Script นี้
/// 2. แนบ Collider2D แล้วเปิด ✅ Is Trigger
/// 3. ตรวจสอบว่า Player มี Tag "Player"
/// </summary>
public class WinZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.Win();
        else
            Debug.LogWarning("[WinZone] ไม่พบ GameManager.Instance!");
    }
}