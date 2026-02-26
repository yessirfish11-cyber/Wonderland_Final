using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Enemy Destroyer พร้อม Animation Idle และ Attack
/// </summary>
public class EnemyDestroyer : MonoBehaviour
{
    [Header("Hiding Spot Destruction")]
    public float destructionInterval = 3f;
    public string hidingSpotTag = "HidingSpot";

    [Header("Destruction Sprites (ต้องลากตามลำดับ!)")]
    [Tooltip("ลาก Sprite ทั้ง 5 ตัวตามลำดับ:\n0 = ปกติ\n1 = แตกน้อย\n2 = แตกกลาง\n3 = แตกเยอะ\n4 = เศษกระจาย")]
    public Sprite[] destructionSprites = new Sprite[5];
    
    [Tooltip("เวลาแสดงแต่ละ Frame (วินาที)")]
    public float frameTime = 0.08f;

    [Header("Enemy Animation")]
    [Tooltip("Animator ของ Enemy")]
    public Animator animator;
    
    [Tooltip("ชื่อ Parameter Bool ใน Animator (true = Attack, false = Idle)")]
    public string attackParamName = "IsAttacking";
    
    [Tooltip("ชื่อ Trigger สำหรับเริ่มโจมตี (Optional)")]
    public string attackTriggerName = "Attack";

    [Header("Effect (Optional)")]
    public GameObject destructionEffectPrefab;
    
    [Header("Sound (Optional)")]
    public AudioClip destructionSound;
    private AudioSource audioSource;

    private PlayerMiniGame4 player;
    private Coroutine destructionCoroutine;
    private List<GameObject> availableHidingSpots = new List<GameObject>();

    void Start()
    {
        // หา Components
        if (animator == null)
            animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && destructionSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        player = FindFirstObjectByType<PlayerMiniGame4>();
        
        // เช็คว่าใส่ Sprites ครบหรือไม่
        if (destructionSprites.Length < 5)
        {
            Debug.LogError("[EnemyDestroyer] ต้องใส่ Sprite ครบ 5 ตัว!");
        }
        else
        {
            for (int i = 0; i < destructionSprites.Length; i++)
            {
                if (destructionSprites[i] == null)
                    Debug.LogWarning($"[EnemyDestroyer] Sprite ช่องที่ {i} ยังว่างอยู่!");
            }
        }

        // ตั้งเป็น Idle ตอนเริ่ม
        SetAnimationState(false);
    }

    void Update()
    {
        if (GameManager.isPaused) return;
        CheckPlayerHidingStatus();
    }

    void CheckPlayerHidingStatus()
    {
        if (player == null) return;
        bool playerIsHiding = player.IsHiding();

        if (playerIsHiding && destructionCoroutine == null)
        {
            Debug.Log("[EnemyDestroyer] Start destroying...");
            destructionCoroutine = StartCoroutine(DestroyHidingSpotsCoroutine());
        }
        else if (!playerIsHiding && destructionCoroutine != null)
        {
            StopCoroutine(destructionCoroutine);
            destructionCoroutine = null;
            
            // กลับเป็น Idle
            SetAnimationState(false);
        }
    }

    IEnumerator DestroyHidingSpotsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(destructionInterval);
            FindAvailableHidingSpots();

            if (availableHidingSpots.Count == 0)
            {
                SetAnimationState(false); // Idle
                yield break;
            }

            GameObject spot = availableHidingSpots[Random.Range(0, availableHidingSpots.Count)];
            
            // เริ่มโจมตี → เล่น Attack Animation
            yield return StartCoroutine(PlayAttackSequence(spot));
            
            // กลับเป็น Idle
            SetAnimationState(false);
        }
    }

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
                    availableHidingSpots.Add(spot);
            }
        }
    }

    // ─────────────────────────────────────────
    // เล่น Attack Animation + ทำลาย Object
    IEnumerator PlayAttackSequence(GameObject spot)
    {
        // 1. เริ่ม Attack Animation
        SetAnimationState(true);
        
        // 2. รอให้ Animation เล่นนิดหน่อย (ถ้ามี)
        yield return new WaitForSeconds(0.2f);
        
        // 3. ทำลาย Object พร้อม Animation
        yield return StartCoroutine(PlayDestructionAnimation(spot));
    }

    // ─────────────────────────────────────────
    // เปลี่ยน Sprite ของ Hiding Spot ทีละ Frame
    IEnumerator PlayDestructionAnimation(GameObject spot)
    {
        if (spot == null) yield break;

        Debug.Log($"[EnemyDestroyer] 💥 ทำลาย '{spot.name}'!");

        // เสียง
        if (audioSource != null && destructionSound != null)
            audioSource.PlayOneShot(destructionSound);

        // เช็ค Player
        InteractableObjectMG4 interactable = spot.GetComponent<InteractableObjectMG4>();
        if (interactable != null && interactable.IsPlayerHidingHere())
        {
            Debug.Log($"[EnemyDestroyer] ⚠️ Player forced out from '{spot.name}'!");
            interactable.ForcePlayerOut();
        }

        // ดึง SpriteRenderer
        SpriteRenderer sr = spot.GetComponent<SpriteRenderer>();
        
        if (sr == null)
        {
            Debug.LogError($"[EnemyDestroyer] '{spot.name}' ไม่มี SpriteRenderer!");
            Destroy(spot);
            yield break;
        }

        // ─── เล่น Animation: เปลี่ยน Sprite ทีละตัว ───
        if (destructionSprites.Length >= 5)
        {
            for (int i = 0; i < destructionSprites.Length; i++)
            {
                if (destructionSprites[i] != null)
                {
                    sr.sprite = destructionSprites[i];
                    Debug.Log($"[EnemyDestroyer] Frame {i}: {destructionSprites[i].name}");
                    yield return new WaitForSeconds(frameTime);
                }
            }

            // Fade out
            Color startColor = sr.color;
            float fadeDuration = frameTime * 2;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[EnemyDestroyer] Sprites ไม่ครบ! ข้าม animation");
            yield return new WaitForSeconds(0.3f);
        }

        // Particle
        if (destructionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                destructionEffectPrefab, 
                spot.transform.position, 
                Quaternion.identity
            );
            Destroy(effect, 2f);
        }

        // ทำลาย Object
        Destroy(spot);
    }

    // ─────────────────────────────────────────
    // ตั้งค่า Animation State
    void SetAnimationState(bool isAttacking)
    {
        if (animator == null) return;

        // ตั้ง Bool Parameter
        if (!string.IsNullOrEmpty(attackParamName))
        {
            animator.SetBool(attackParamName, isAttacking);
        }

        // ยิง Trigger (ถ้ามี)
        if (isAttacking && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        Debug.Log($"[EnemyDestroyer] Animation: {(isAttacking ? "ATTACK" : "IDLE")}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}