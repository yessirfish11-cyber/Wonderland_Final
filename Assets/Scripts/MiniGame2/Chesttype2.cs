using UnityEngine;

public class ChestType2 : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Room Chest Settings")]
    [SerializeField] private int totalChestsInRoom = 3;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] spawnPrefabs;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.5f);
    [SerializeField] private float spawnScatter = 0.5f;
    [SerializeField] private int spawnAmountMin = 1;
    [SerializeField] private int spawnAmountMax = 1;
    [SerializeField] private float spawnedObjectLifetime = 0f;

    [Header("UI Hint (Optional)")]
    [SerializeField] private GameObject interactHint;

    private static int openedCount = 0;

    private Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private static readonly int AnimIsOpen = Animator.StringToHash("IsOpen");

    private void OnEnable()
    {
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
        Debug.Log($"[ChestType2] เปิดกล่อง: {openedCount}/{totalChestsInRoom}");

        if (openedCount >= totalChestsInRoom)
        {
            Debug.Log("[ChestType2] กล่องสุดท้าย! → Spawn Object");
            SpawnObjects();
        }
    }

    private void SpawnObjects()
    {
        if (spawnPrefabs == null || spawnPrefabs.Length == 0) return;
        int amount = Random.Range(spawnAmountMin, spawnAmountMax + 1);
        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            Vector2 pos = (Vector2)transform.position + spawnOffset
                        + (spawnScatter > 0f ? Random.insideUnitCircle * spawnScatter : Vector2.zero);
            GameObject spawned = Instantiate(prefab, pos, Quaternion.identity);
            if (spawnedObjectLifetime > 0f) Destroy(spawned, spawnedObjectLifetime);
        }
    }

    public void OnChestFullyOpened() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}