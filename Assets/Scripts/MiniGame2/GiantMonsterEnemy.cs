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

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    private EnemyState currentState = EnemyState.Patrol;
    private int currentWaypointIndex = 0;
    private Transform player;
    private StealthPlayer playerScript;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private int lastDirectionState = 2;

    public bool IsChasing => currentState == EnemyState.Chase;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<StealthPlayer>();
        }

        currentState = EnemyState.Patrol;
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

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

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
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
        }
    }

    private void DetectPlayer()
    {
        if (player == null || playerScript == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        float nearRadius = visionRange * 0.5f;

        // --- วงใกล้ (nearRadius = visionRange / 2) ---
        // ตรวจจับทันที ไม่ว่าผู้เล่นจะหยุดหรือขยับ ไม่มีข้อจำกัดมุมมอง
        if (distanceToPlayer <= nearRadius)
        {
            RaycastHit2D hitNear = Physics2D.Raycast(
                transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
            if (hitNear.collider == null)
            {
                StartChase();
                return;
            }
        }

        // --- วงไกล (visionRange + visionAngle) ---
        // ตรวจจับเฉพาะเมื่อผู้เล่นกำลังเดินหรือวิ่ง
        if (distanceToPlayer > visionRange) return;

        Vector2 forward = GetForwardDirection();
        float angle = Vector2.Angle(forward, directionToPlayer);
        if (angle > visionAngle) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null) return;

        // IsMoving ต้องมีใน StealthPlayer (ทั้งเดินและวิ่งถือว่า true)
        if (playerScript.IsMoving)
        {
            StartChase();
        }
    }

    private void StartChase()
    {
        currentState = EnemyState.Chase;
        Debug.Log("ตัวเงินตัวทองยักษ์เห็นผู้เล่น! กำลังไล่ล่า!");

        if (audioSource != null && chaseMusic != null)
        {
            audioSource.clip = chaseMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

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

        // ถ้าผู้เล่นอยู่ในวงใกล้ ยังคงไล่ต่อเสมอ ไม่หยุด
        if (distanceToPlayer <= nearRadius) return;

        // ถ้าผู้เล่นออกมานอกวงใกล้ และหยุดขยับ ให้หยุดไล่
        if (playerScript != null && !playerScript.IsMoving)
        {
            StopChase();
        }
    }

    private void StopChase()
    {
        currentState = EnemyState.Patrol;
        Debug.Log("หลุดสายตา กลับไปลาดตระเวน");

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void AttackPlayer()
    {
        movement = Vector2.zero;

        // หยุดเพลงไล่ล่าก่อน
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // เล่นเสียงจับได้ (ถ้ามี) — JumpScareUI จะเล่นเสียง Jumpscare ต่อเอง
        if (audioSource != null && catchSound != null)
            audioSource.PlayOneShot(catchSound);

        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        // แอนิเมชัน Attack
        if (animator != null)
            animator.SetTrigger("Attack");

        // หยุดการเคลื่อนที่ของผู้เล่นทันที
        if (playerScript != null)
            playerScript.SetMovement(false);

        // รอนิดนึงให้แอนิเมชัน Attack เริ่มต้น
        yield return new WaitForSecondsRealtime(0.3f);

        // ──── เรียก Jump Scare UI ────
        if (JumpScareUI.Instance != null)
        {
            JumpScareUI.Instance.TriggerJumpScare();
        }
        else
        {
            Debug.LogWarning("JumpScareUI.Instance เป็น null — ตรวจสอบว่ามี JumpScareUI ใน Scene");
        }

        // Enemy หยุดนิ่ง ไม่ต้อง return to Patrol เพราะ Scene จะ Reload
    }

    private Vector2 GetForwardDirection()
    {
        if (movement.magnitude > 0.1f)
        {
            return movement.normalized;
        }

        switch (lastDirectionState)
        {
            case 1: return Vector2.up;
            case 2: return Vector2.down;
            case 3: return Vector2.left;
            case 4: return Vector2.right;
            default: return Vector2.down;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (movement.magnitude > 0.1f)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirectionState = movement.x > 0 ? 4 : 3;
            }
            else
            {
                lastDirectionState = movement.y > 0 ? 1 : 2;
            }

            animator.SetInteger("State", lastDirectionState);
        }
        else
        {
            animator.SetInteger("State", lastDirectionState * 10);
        }

        animator.SetFloat("Speed", movement.magnitude);
        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
    }

    private void OnDrawGizmos()
    {
        // วงไกล (visionRange) - เหลือง = ลาดตระเวน, แดง = กำลังไล่
        Gizmos.color = currentState == EnemyState.Chase ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // วงใกล้ (visionRange * 0.5f) - สีส้ม: ตรวจจับทันทีไม่ว่าผู้เล่นจะหยุดหรือขยับ
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, visionRange * 0.3f);

        // มุมมองเห็น (เฉพาะวงไกล)
        Vector2 forward = GetForwardDirection();
        Vector3 rightBound = Quaternion.Euler(0, 0, -visionAngle) * forward;
        Vector3 leftBound = Quaternion.Euler(0, 0, visionAngle) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rightBound * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftBound * visionRange);

        // ระยะจับ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        // เส้นทางลาดตระเวน
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