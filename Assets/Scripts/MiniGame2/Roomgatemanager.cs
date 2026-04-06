using UnityEngine;
using System.Collections;

/// <summary>
/// จัดการประตู/สิ่งกีดขวางที่เปิดได้เมื่อ ChestType1 เปิดครบ
///
/// วิธีใช้:
/// 1. สร้าง GameObject ใหม่ใน Scene แนบ Script นี้
/// 2. **ตั้ง Tag ของ GameObject นี้ว่า "RoomGateManager"**
/// 3. ลาก GameObject ที่เป็นประตู/กำแพงใส่ช่อง gateObjects
/// </summary>
public class RoomGateManager : MonoBehaviour
{
    [Header("Gate Objects")]
    [Tooltip("ลาก GameObject ที่เป็นประตู/กำแพงกั้นทางใส่ที่นี่")]
    [SerializeField] private GameObject[] gateObjects;

    public enum GateOpenMode { Disable, Destroy, Animate }
    [SerializeField] private GateOpenMode openMode = GateOpenMode.Disable;

    [SerializeField] private float openDelay = 0.5f;

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject openEffect;
    [SerializeField] private AudioClip openSound;

    private AudioSource audioSource;
    private bool isGateOpen = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OpenGate()
    {
        if (isGateOpen) return;
        isGateOpen = true;
        StartCoroutine(OpenGateRoutine());
    }

    private IEnumerator OpenGateRoutine()
    {
        yield return new WaitForSeconds(openDelay);

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        foreach (var gate in gateObjects)
        {
            if (gate == null) continue;

            if (openEffect != null)
                Instantiate(openEffect, gate.transform.position, Quaternion.identity);

            switch (openMode)
            {
                case GateOpenMode.Disable:  gate.SetActive(false); break;
                case GateOpenMode.Destroy:  Destroy(gate); break;
                case GateOpenMode.Animate:
                    Animator anim = gate.GetComponent<Animator>();
                    if (anim != null) anim.SetBool("IsOpen", true);
                    else gate.SetActive(false);
                    break;
            }
        }

        Debug.Log("[RoomGateManager] ทางเดินถูกเปิดแล้ว!");
    }

    public void ResetGate()
    {
        isGateOpen = false;
        foreach (var gate in gateObjects)
        {
            if (gate == null) continue;
            gate.SetActive(true);
            Animator anim = gate.GetComponent<Animator>();
            if (anim != null) anim.SetBool("IsOpen", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (gateObjects == null) return;
        Gizmos.color = Color.green;
        foreach (var gate in gateObjects)
            if (gate != null)
                Gizmos.DrawWireCube(gate.transform.position, gate.transform.localScale);
    }
}