using UnityEngine;

public enum InteractTypeMG4
{
    Hide,       // ซ่อนตัว (กด E)
    Collect     // เก็บไอเทม (เดินชนเก็บอัตโนมัติ)
}

public class InteractableObjectMG4 : MonoBehaviour
{
    [Header("Interaction Type")]
    public InteractTypeMG4 interactType = InteractTypeMG4.Collect;

    [Header("Visual Hint")]
    public GameObject interactPrompt; // ป้าย [E] สำหรับ Hide เท่านั้น

    [Header("Collect Settings")]
    public bool destroyAfterCollect = true;

    private PlayerMiniGame4 playerInRange = null;
    private bool playerIsHidingHere = false;
    private bool isCollected = false;

    void Update()
    {
        // แสดง prompt เฉพาะ Hide type
        if (interactPrompt != null)
        {
            bool shouldShow = (interactType == InteractTypeMG4.Hide) && 
                              (playerInRange != null);
            interactPrompt.SetActive(shouldShow);
        }
    }

    // ─────────────────────────────────────────
    // สำหรับ Hide - เรียกจาก Player เมื่อกด E
    public void Interact(PlayerMiniGame4 player)
    {
        if (interactType == InteractTypeMG4.Hide)
        {
            HandleHide(player);
        }
        // Collect type ไม่ใช้ฟังก์ชันนี้ เพราะเก็บอัตโนมัติใน OnTriggerEnter2D
    }

    // ─────────────────────────────────────────
    void HandleHide(PlayerMiniGame4 player)
    {
        if (!player.IsHiding())
        {
            // ซ่อนตัว
            player.SetHiding(true);
            player.transform.position = transform.position;
            playerIsHidingHere = true;
            playerInRange = player;
            Debug.Log($"[Interact] Player hiding in '{gameObject.name}'");
        }
        else
        {
            // ออกจากที่ซ่อน
            player.SetHiding(false);
            playerIsHidingHere = false;
            playerInRange = null;
            Debug.Log($"[Interact] Player came out from '{gameObject.name}'");
        }
    }

    // ─────────────────────────────────────────
    void HandleCollect(PlayerMiniGame4 player)
    {
        if (isCollected) return;

        isCollected = true;
        player.CollectItem();
        Debug.Log($"[Interact] Item '{gameObject.name}' collected!");

        if (destroyAfterCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMiniGame4 player = other.GetComponent<PlayerMiniGame4>();
        if (player == null) return;

        playerInRange = player;

        // ถ้าเป็น Collect type → เก็บทันที!
        if (interactType == InteractTypeMG4.Collect)
        {
            HandleCollect(player);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // สำหรับ Hide type - ถ้า Player ออกจากพื้นที่ขณะซ่อนอยู่
        if (interactType == InteractTypeMG4.Hide)
        {
            if (playerIsHidingHere && playerInRange != null)
            {
                playerInRange.SetHiding(false);
                playerIsHidingHere = false;
            }
        }

        playerInRange = null;
    }
}