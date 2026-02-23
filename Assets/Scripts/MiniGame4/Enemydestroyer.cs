using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Enemy ที่อยู่นิ่งๆ แต่ทำลายที่ซ่อนแบบสุ่มเมื่อ Player ซ่อนอยู่
/// รองรับ Particle Effect และ Animation ตอนทำลาย
/// </summary>
public class EnemyDestroyer : MonoBehaviour
{
    [Header("Hiding Spot Destruction")]
    [Tooltip("เวลาระหว่างการทำลายแต่ละที่ซ่อน (วินาที)")]
    public float destructionInterval = 3f;
    
    [Tooltip("Tag ของ Hiding Spots (ควรเป็น 'HidingSpot')")]
    public string hidingSpotTag = "HidingSpot";

    [Header("Destruction Effect")]
    [Tooltip("Particle System Prefab (เช่น ควันระเบิด, เศษไม้)")]
    public GameObject destructionEffectPrefab;
    
    [Tooltip("Animation Clip ที่เล่นตอนทำลาย (ถ้าใช้ Animation แทน Particle)")]
    public AnimationClip destructionAnimation;
    
    [Tooltip("ระยะเวลารอให้ Animation เล่นจบก่อนทำลาย Object (วินาที)")]
    public float destroyDelay = 0.5f;

    [Header("Sound (Optional)")]
    public AudioClip destructionSound;
    private AudioSource audioSource;

    [Header("Enemy Animation (Optional)")]
    [Tooltip("Animator ของ Enemy เอง")]
    public Animator enemyAnimator;
    public string attackTrigger = "Attack";

    // Destruction system
    private PlayerMiniGame4 player;
    private Coroutine destructionCoroutine;
    private List<GameObject> availableHidingSpots = new List<GameObject>();

    // ─────────────────────────────────────────
    void Start()
    {
        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && destructionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // หา Player
        player = FindFirstObjectByType<PlayerMiniGame4>();
        if (player == null)
            Debug.LogError("[EnemyDestroyer] ไม่เจอ PlayerMiniGame4 ใน Scene!");
        else
            Debug.Log($"[EnemyDestroyer] '{gameObject.name}' พร้อมแล้ว!");
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (GameManager.isPaused) return;
        CheckPlayerHidingStatus();
    }

    // ─────────────────────────────────────────
    void CheckPlayerHidingStatus()
    {
        if (player == null) return;

        bool playerIsHiding = player.IsHiding();

        if (playerIsHiding && destructionCoroutine == null)
        {
            Debug.Log($"[EnemyDestroyer] '{gameObject.name}' detected hiding! Start destroying...");
            destructionCoroutine = StartCoroutine(DestroyHidingSpotsCoroutine());
        }
        else if (!playerIsHiding && destructionCoroutine != null)
        {
            Debug.Log($"[EnemyDestroyer] '{gameObject.name}' Player came out! Stop destroying.");
            StopCoroutine(destructionCoroutine);
            destructionCoroutine = null;
        }
    }

    // ─────────────────────────────────────────
    IEnumerator DestroyHidingSpotsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(destructionInterval);

            FindAvailableHidingSpots();

            if (availableHidingSpots.Count == 0)
            {
                Debug.Log("[EnemyDestroyer] ไม่มีที่ซ่อนเหลืออยู่แล้ว!");
                yield break;
            }

            // เล่น Enemy animation
            PlayEnemyAttackAnimation();

            // สุ่มทำลาย
            GameObject spotToDestroy = availableHidingSpots[Random.Range(0, availableHidingSpots.Count)];
            StartCoroutine(DestroyHidingSpotWithEffect(spotToDestroy));
        }
    }

    // ─────────────────────────────────────────
    void FindAvailableHidingSpots()
    {
        availableHidingSpots.Clear();

        GameObject[] spots = GameObject.FindGameObjectsWithTag(hidingSpotTag);
        
        foreach (GameObject spot in spots)
        {
            if (spot.activeInHierarchy)
            {
                InteractableObjectMG4 interactable = spot.GetComponent<InteractableObjectMG4>();
                if (interactable != null && interactable.interactType == InteractTypeMG4.Hide)
                {
                    availableHidingSpots.Add(spot);
                }
            }
        }

        Debug.Log($"[EnemyDestroyer] พบที่ซ่อนทั้งหมด: {availableHidingSpots.Count} แห่ง");
    }

    // ─────────────────────────────────────────
    // ทำลายพร้อม Effect
    IEnumerator DestroyHidingSpotWithEffect(GameObject spot)
    {
        if (spot == null) yield break;

        Debug.Log($"[EnemyDestroyer] 💥 ทำลาย '{spot.name}'!");

        // เล่นเสียง
        PlayDestructionSound();

        // เช็คว่า Player ซ่อนอยู่ที่นี่หรือไม่
        InteractableObjectMG4 interactable = spot.GetComponent<InteractableObjectMG4>();
        bool playerWasHere = false;

        if (interactable != null && interactable.IsPlayerHidingHere())
        {
            Debug.Log($"[EnemyDestroyer] ⚠️ Player was hiding in '{spot.name}'! Forced out!");
            interactable.ForcePlayerOut();
            playerWasHere = true;
        }

        // ─── วิธีที่ 1: ใช้ Animation Clip (ถ้ามี) ───
        if (destructionAnimation != null)
        {
            Animator spotAnimator = spot.GetComponent<Animator>();
            if (spotAnimator != null)
            {
                spotAnimator.Play(destructionAnimation.name);
                Debug.Log($"[EnemyDestroyer] เล่น Animation: {destructionAnimation.name}");
            }
            
            // รอให้ Animation เล่นจบ
            yield return new WaitForSeconds(destroyDelay);
        }

        // ─── วิธีที่ 2: ใช้ Particle Effect ───
        if (destructionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                destructionEffectPrefab, 
                spot.transform.position, 
                Quaternion.identity
            );
            
            // ทำลาย Particle หลังเล่นจบ
            Destroy(effect, 2f);
        }

        // ทำลาย Object
        Destroy(spot);

        if (!playerWasHere)
        {
            Debug.Log($"[EnemyDestroyer] '{spot.name}' destroyed, but player wasn't here.");
        }
    }

    // ─────────────────────────────────────────
    void PlayEnemyAttackAnimation()
    {
        if (enemyAnimator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            enemyAnimator.SetTrigger(attackTrigger);
        }
    }

    void PlayDestructionSound()
    {
        if (audioSource != null && destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound);
        }
    }

    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}