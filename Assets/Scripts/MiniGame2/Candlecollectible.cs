using UnityEngine;

public class CandleCollectible : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Wall")]
    [SerializeField] private int totalCandlesInRoom = 3;
    [SerializeField] private string wallManagerTag = "WallManager";

    [Header("UI Hint (Optional)")]
    [SerializeField] private GameObject interactHint;

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;

    private static int collectedCount = 0;

    private bool isCollected = false;
    private bool playerInRange = false;
    private Transform playerTransform;

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
        collectedCount = 0;
    }

    private void Awake()
    {
        if (interactHint != null) interactHint.SetActive(false);
    }

    private void Start()
    {
        collectedCount = 0;
    }

    private void Update()
    {
        if (isCollected) return;
        CheckPlayerRange();
        if (playerInRange && Input.GetKeyDown(interactKey)) Collect();
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
            if (interactHint != null) interactHint.SetActive(playerInRange);
        }
    }

    private void Collect()
    {
        isCollected = true;
        collectedCount++;
        if (interactHint != null) interactHint.SetActive(false);
        if (collectSound != null) AudioSource.PlayClipAtPoint(collectSound, transform.position);
        if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);
        Debug.Log($"[Candle] เก็บเทียน: {collectedCount}/{totalCandlesInRoom}");

        if (collectedCount >= totalCandlesInRoom)
        {
            GameObject wallObj = GameObject.FindGameObjectWithTag(wallManagerTag);
            WallManager wm = wallObj != null
                ? wallObj.GetComponent<WallManager>()
                : FindObjectOfType<WallManager>();
            if (wm != null) wm.RemoveWall();
            else Debug.LogWarning("[Candle] หา WallManager ไม่เจอ!");
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}