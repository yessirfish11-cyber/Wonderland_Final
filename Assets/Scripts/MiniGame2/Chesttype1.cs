using UnityEngine;

public class ChestType1 : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Room Gate")]
    [SerializeField] private int totalChestsInRoom = 3;
    [SerializeField] private string gateManagerTag = "RoomGateManager";

    [Header("UI Hint (Optional)")]
    [SerializeField] private GameObject interactHint;

    // ── ใช้ instance counter แทน static ──
    private static int openedCount = 0;

    private Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private static readonly int AnimIsOpen = Animator.StringToHash("IsOpen");

    private void OnEnable()
    {
        // Reset ทุกครั้งที่ Scene โหลด (เรียกผ่าน SceneManager event)
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        openedCount = 0;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (interactHint != null) interactHint.SetActive(false);
    }

    private void Start()
    {
        // Reset ทุกครั้งที่ Start (ครอบคลุม Restart)
        openedCount = 0;
    }

    private void Update()
    {
        CheckPlayerRange();
        if (playerInRange && !isOpen && Input.GetKeyDown(interactKey))
            OpenChest();
    }

    private void CheckPlayerRange()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRange;
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (interactHint != null) interactHint.SetActive(playerInRange && !isOpen);
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        openedCount++;
        if (animator != null) animator.SetBool(AnimIsOpen, true);
        if (interactHint != null) interactHint.SetActive(false);

        if (openedCount >= totalChestsInRoom)
        {
            GameObject gateObj = GameObject.FindGameObjectWithTag(gateManagerTag);
            RoomGateManager gm = gateObj != null
                ? gateObj.GetComponent<RoomGateManager>()
                : FindObjectOfType<RoomGateManager>();
        }
    }

    public void OnChestFullyOpened() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}