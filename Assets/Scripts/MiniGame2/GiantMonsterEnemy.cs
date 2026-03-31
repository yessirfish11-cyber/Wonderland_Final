using UnityEngine;
using System.Collections;

public class GiantMonsterEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointThreshold = 0.5f;

    [Header("Detection")]
    [SerializeField] private float visionRange = 8f;
    [SerializeField] private float visionAngle = 60f;
    [SerializeField] private float catchDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private AudioClip catchSound;

    [Header("Proximity Audio")]
    [SerializeField] private AudioSource proximityAudioSource;  // AudioSource แยกสำหรับเสียงใกล้
    [SerializeField] private AudioClip proximitySound;          // เสียงที่จะ Loop เมื่อใกล้
    [SerializeField] private float proximitySoundRange = 12f;   // ระยะที่เริ่มได้ยินเสียง
    [SerializeField] private float proximitySoundMaxVolume = 1f;// ความดังสูงสุด
    [SerializeField] private float proximitySoundFadeSpeed = 2f;// ความเร็วในการ Fade

    [Header("Sprite Direction")]
    [SerializeField] private bool defaultFacingRight = true;

    private enum EnemyState { Patrol, Chase, Attack }

    private EnemyState currentState = EnemyState.Patrol;
    private int currentWaypointIndex = 0;
    private Transform player;
    private StealthPlayer playerScript;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private int lastDirectionState = 2;

    // ── Proximity audio state ──
    private float targetProximityVolume = 0f;

    // ── Attack guard ──
    private bool hasAttacked = false;

    public bool IsChasing => currentState == EnemyState.Chase;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // ตั้งค่า proximityAudioSource (ต้องกำหนดใน Inspector)
        if (proximityAudioSource != null)
        {
            proximityAudioSource.loop = true;
            proximityAudioSource.playOnAwake = false;
            proximityAudioSource.volume = 0f;
            proximityAudioSource.spatialBlend = 0f;
        }
        else
        {
            Debug.LogWarning("[GiantMonster] กรุณาใส่ Proximity Audio Source ใน Inspector!");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<StealthPlayer>();
        }

        currentState = EnemyState.Patrol;

        // เริ่ม Proximity Loop ทันที (Volume = 0 ก่อน)
        StartProximityLoop();
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                DetectPlayer();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }

        UpdateSpriteFlip();
        UpdateProximityVolume();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    // ════════════════════════════════════════════
    //  PROXIMITY AUDIO
    // ════════════════════════════════════════════

    private void StartProximityLoop()
    {
        if (proximityAudioSource == null || proximitySound == null) return;

        proximityAudioSource.clip = proximitySound;
        proximityAudioSource.loop = true;
        proximityAudioSource.volume = 0f;
        proximityAudioSource.Play();
    }

    private void UpdateProximityVolume()
    {
        if (proximityAudioSource == null || proximitySound == null || player == null) return;

        // ถ้ากำลัง Chase หรือ Attack ให้ปิดเสียง proximity (เพราะ chaseMusic จะเล่นแทน)
        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            targetProximityVolume = 0f;
        }
        else
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance >= proximitySoundRange)
            {
                targetProximityVolume = 0f;
            }
            else
            {
                // ยิ่งใกล้ยิ่งดัง: Volume = 1 เมื่อ distance = 0, Volume = 0 เมื่อ distance = proximitySoundRange
                float ratio = 1f - (distance / proximitySoundRange);
                targetProximityVolume = ratio * proximitySoundMaxVolume;
            }
        }

        // Smooth Fade
        proximityAudioSource.volume = Mathf.MoveTowards(
            proximityAudioSource.volume,
            targetProximityVolume,
            proximitySoundFadeSpeed * Time.deltaTime
        );
    }

    // ════════════════════════════════════════════
    //  PATROL & DETECTION
    // ════════════════════════════════════════════

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 targetPos = patrolPoints[currentWaypointIndex].position;
        Vector2 direction = (targetPos - rb.position).normalized;
        movement = direction * patrolSpeed;

        float distance = Vector2.Distance(rb.position, targetPos);
        if (distance < waypointThreshold)
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
    }

    private void DetectPlayer()
    {
        if (player == null || playerScript == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float nearRadius = visionRange * 0.5f;

        if (distanceToPlayer <= nearRadius)
        {
            RaycastHit2D hitNear = Physics2D.Raycast(
                transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
            if (hitNear.collider == null) { StartChase(); return; }
        }

        if (distanceToPlayer > visionRange) return;

        Vector2 forward = GetForwardDirection();
        float angle = Vector2.Angle(forward, directionToPlayer);
        if (angle > visionAngle) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null) return;

        if (playerScript.IsMoving) StartChase();
    }

    private void StartChase()
    {
        currentState = EnemyState.Chase;

        if (audioSource != null && chaseMusic != null)
        {
            audioSource.clip = chaseMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // ════════════════════════════════════════════
    //  CHASE & ATTACK
    // ════════════════════════════════════════════

    private void ChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        if (distanceToPlayer < catchDistance)
        {
            currentState = EnemyState.Attack;
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        movement = direction * chaseSpeed;

        float nearRadius = visionRange * 0.5f;
        if (distanceToPlayer <= nearRadius) return;

        if (playerScript != null && !playerScript.IsMoving)
            StopChase();
    }

    private void StopChase()
    {
        currentState = EnemyState.Patrol;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void AttackPlayer()
    {
        if (hasAttacked) return;
        hasAttacked = true;

        movement = Vector2.zero;

        Debug.Log($"[Attack] audioSource={audioSource}, catchSound={catchSound}");

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (audioSource != null && catchSound != null)
        {
            Debug.Log("[Attack] PlayOneShot catchSound");
            audioSource.PlayOneShot(catchSound);
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("[Attack] audioSource เป็น null!");
            if (catchSound == null) Debug.LogWarning("[Attack] catchSound เป็น null!");
        }

        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        if (playerScript != null)
            playerScript.SetMovement(false);

        yield return new WaitForSecondsRealtime(0.3f);

        if (GameOverUI2.Instance != null)
            GameOverUI2.Instance.Show();
        else
            Debug.LogWarning("GameOverUI.Instance เป็น null — ตรวจสอบว่ามี GameOverUI ใน Scene");
    }

    // ════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════

    private Vector2 GetForwardDirection()
    {
        if (movement.magnitude > 0.1f) return movement.normalized;

        switch (lastDirectionState)
        {
            case 1: return Vector2.up;
            case 2: return Vector2.down;
            case 3: return Vector2.left;
            case 4: return Vector2.right;
            default: return Vector2.down;
        }
    }

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(movement.x) > 0.1f)
        {
            bool movingRight = movement.x > 0f;
            spriteRenderer.flipX = defaultFacingRight ? !movingRight : movingRight;
            lastDirectionState = movingRight ? 4 : 3;
        }
        else if (Mathf.Abs(movement.y) > 0.1f)
        {
            lastDirectionState = movement.y > 0f ? 1 : 2;
        }
    }

    // ════════════════════════════════════════════
    //  GIZMOS
    // ════════════════════════════════════════════

    private void OnDrawGizmos()
    {
        Gizmos.color = currentState == EnemyState.Chase ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, visionRange * 0.3f);

        // Proximity sound range (สีม่วง)
        Gizmos.color = new Color(0.6f, 0f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, proximitySoundRange);

        Vector2 forward = GetForwardDirection();
        Vector3 rightBound = Quaternion.Euler(0, 0, -visionAngle) * forward;
        Vector3 leftBound  = Quaternion.Euler(0, 0,  visionAngle) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rightBound * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftBound  * visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Vector3 current = patrolPoints[i].position;
                    Vector3 next = patrolPoints[(i + 1) % patrolPoints.Length].position;
                    Gizmos.DrawLine(current, next);
                    Gizmos.DrawWireSphere(current, 0.3f);
                }
            }
        }
    }
}