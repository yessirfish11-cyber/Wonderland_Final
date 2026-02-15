using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("คะแนนที่ได้เมื่อเก็บ Item นี้")]
    [SerializeField] private int scoreValue = 1;

    [Tooltip("ชื่อของ Item (แสดงใน Prompt)")]
    [SerializeField] private string itemName = "แผนที่";

    [Header("Prompt UI")]
    [Tooltip("GameObject ที่ครอบ Text แจ้งให้กด E (โชว์/ซ่อนอัตโนมัติ)")]
    [SerializeField] private GameObject promptUI;

    [Tooltip("TMP_Text สำหรับข้อความ Prompt (ถ้าไม่กำหนดจะหา Component ใน promptUI)")]
    [SerializeField] private TMP_Text promptText;

    [Tooltip("ข้อความ Prompt ใช้ {name} แทนชื่อ Item")]
    [SerializeField] private string promptMessage = "[E] เก็บ {name}";

    [Header("Collect Effect")]
    [Tooltip("Particle / VFX ตอนเก็บ Item (ถ้ามี)")]
    [SerializeField] private GameObject collectVFX;

    [Tooltip("เสียงตอนเก็บ Item")]
    [SerializeField] private AudioClip collectSound;

    [Tooltip("ระยะ Fade Out ก่อน Destroy (วินาที, ใส่ 0 = ลบทันที)")]
    [SerializeField] private float fadeOutDuration = 0.3f;

    // ─────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────

    private bool playerInRange = false;
    private bool collected     = false;
    private AudioSource audioSource;

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // หา / สร้าง AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // หา TMP_Text ใน promptUI อัตโนมัติ
        if (promptText == null && promptUI != null)
            promptText = promptUI.GetComponentInChildren<TMP_Text>();

        SetPromptVisible(false);
    }

    private void Update()
    {
        // ไม่รับ Input ถ้าเก็บไปแล้ว หรือ Game pause
        if (collected || GameManager.isPaused) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    // ─────────────────────────────────────────────
    // Trigger — ตรวจจับผู้เล่นเข้า/ออกระยะ
    // ─────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        playerInRange = true;
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        SetPromptVisible(false);
    }

    // ─────────────────────────────────────────────
    // Core — เก็บ Item
    // ─────────────────────────────────────────────

    private void Collect()
    {
        collected = true;
        SetPromptVisible(false);

        // ─ เพิ่มคะแนนผ่าน GameManager (GameManager จะ trigger Win() เองเมื่อครบ)
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);
        else
            Debug.LogWarning("[CollectibleItem] ไม่พบ GameManager.Instance!");

        // ─ Spawn VFX
        if (collectVFX != null)
            Instantiate(collectVFX, transform.position, Quaternion.identity);

        // ─ เล่นเสียง
        if (collectSound != null)
            audioSource.PlayOneShot(collectSound);

        // ─ Fade แล้ว Destroy
        StartCoroutine(FadeAndDestroy());
    }

    // ─────────────────────────────────────────────
    // Coroutine — Fade Out แล้ว Destroy
    // ─────────────────────────────────────────────

    private IEnumerator FadeAndDestroy()
    {
        // Disable Collider ทันที ป้องกัน Trigger ซ้ำ
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // รอให้เสียงเล่นจบ
        if (collectSound != null)
            yield return new WaitForSeconds(collectSound.length);

        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    // UI Helpers
    // ─────────────────────────────────────────────

    private void SetPromptVisible(bool visible)
    {
        if (promptUI == null) return;
        promptUI.SetActive(visible);

        if (visible && promptText != null)
            promptText.text = promptMessage.Replace("{name}", itemName);
    }


    // ─────────────────────────────────────────────
    // Gizmos — แสดง Trigger Range ใน Scene View
    // ─────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);

        if (col is CircleCollider2D circle)
            Gizmos.DrawWireSphere(transform.position, circle.radius);
        else if (col is BoxCollider2D box)
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
    }
}