using UnityEngine;

/// <summary>
/// ติดกับ GameObject ของ Chest
/// ต้องการ: Animator, SpriteRenderer
/// Animation States: "Idle" (ปิด), "Open" (เปิด)
/// </summary>
public class ChestInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI Hint (Optional)")]
    [SerializeField] private GameObject interactHint; // UI "กด E" ที่แสดงเมื่อเข้าใกล้

    private Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;
    private Transform playerTransform;

    // Animator Parameter Name
    private static readonly int AnimIsOpen = Animator.StringToHash("IsOpen");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // ซ่อน hint ตั้งต้น
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
        CheckPlayerRange();

        if (playerInRange && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void CheckPlayerRange()
    {
        if (playerTransform == null)
        {
            // หา Player อัตโนมัติจาก Tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRange;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;

            // แสดง/ซ่อน hint
            if (interactHint != null)
                interactHint.SetActive(playerInRange && !isOpen);
        }
    }

    private void OpenChest()
    {
        isOpen = true;

        // เล่น Animation เปิดกล่อง
        animator.SetBool(AnimIsOpen, true);

        // ซ่อน hint
        if (interactHint != null)
            interactHint.SetActive(false);

        // TODO: ใส่ Logic ของ Loot / ของรางวัลที่นี่
        Debug.Log($"[Chest] {gameObject.name} ถูกเปิดแล้ว!");
    }

    // เรียกจาก Animation Event เมื่อ Frame สุดท้ายของ Animation "Open" เล่นจบ
    public void OnChestFullyOpened()
    {
        Debug.Log("[Chest] Animation เปิดจบสมบูรณ์");
        // TODO: แสดง Loot Popup หรือ Spawn Item ที่นี่
    }

    // วาด Gizmo แสดง Range ใน Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}