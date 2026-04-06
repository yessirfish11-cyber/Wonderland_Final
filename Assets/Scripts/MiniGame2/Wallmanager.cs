using UnityEngine;
using System.Collections;

/// <summary>
/// จัดการกำแพงที่จะหายเมื่อ CandleCollectible แจ้งว่าเก็บครบ
///
/// วิธีใช้:
/// 1. สร้าง GameObject ใหม่ แนบ Script นี้
/// 2. ลาก GameObject กำแพงใส่ช่อง wallObjects
/// 3. ลาก WallManager นี้ไปใส่ใน CandleCollectible ทุกอัน
/// </summary>
public class WallManager : MonoBehaviour
{
    [Header("Wall Objects")]
    [Tooltip("GameObject ที่เป็นกำแพง — จะถูกซ่อน/ทำลายเมื่อเก็บเทียนครบ")]
    [SerializeField] private GameObject[] wallObjects;

    public enum WallRemoveMode
    {
        Disable,   // SetActive(false)
        Destroy,   // Destroy ทิ้ง
        Animate    // เล่น Animation (ต้องมี Animator + Parameter "IsOpen")
    }
    [SerializeField] private WallRemoveMode removeMode = WallRemoveMode.Disable;

    [Tooltip("หน่วงเวลา (วินาที) ก่อนกำแพงหาย")]
    [SerializeField] private float removeDelay = 0.3f;

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject removeEffect;
    [SerializeField] private AudioClip removeSound;

    private AudioSource audioSource;
    private bool isRemoved = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>เรียกจาก CandleCollectible เมื่อเก็บครบ</summary>
    public void RemoveWall()
    {
        if (isRemoved) return;
        isRemoved = true;
        StartCoroutine(RemoveWallRoutine());
    }

    private IEnumerator RemoveWallRoutine()
    {
        yield return new WaitForSeconds(removeDelay);

        if (audioSource != null && removeSound != null)
            audioSource.PlayOneShot(removeSound);

        foreach (var wall in wallObjects)
        {
            if (wall == null) continue;

            if (removeEffect != null)
                Instantiate(removeEffect, wall.transform.position, Quaternion.identity);

            switch (removeMode)
            {
                case WallRemoveMode.Disable:
                    wall.SetActive(false);
                    break;
                case WallRemoveMode.Destroy:
                    Destroy(wall);
                    break;
                case WallRemoveMode.Animate:
                    Animator anim = wall.GetComponent<Animator>();
                    if (anim != null) anim.SetBool("IsOpen", true);
                    else wall.SetActive(false);
                    break;
            }
        }

        Debug.Log("[WallManager] กำแพงหายแล้ว!");
    }

    /// <summary>รีเซ็ตกำแพงกลับมา (Debug / Respawn)</summary>
    public void ResetWall()
    {
        isRemoved = false;
        foreach (var wall in wallObjects)
        {
            if (wall == null) continue;
            wall.SetActive(true);
            Animator anim = wall.GetComponent<Animator>();
            if (anim != null) anim.SetBool("IsOpen", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wallObjects == null) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f); // สีส้ม
        foreach (var wall in wallObjects)
        {
            if (wall != null)
                Gizmos.DrawWireCube(wall.transform.position, wall.transform.localScale);
        }
    }
}