using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enemy AI ที่ไล่ตามผู้เล่น หรือ Patrol ตามจุดเมื่อผู้เล่นซ่อน
/// </summary>
public class EnemyAi4 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 4f;
    private float currentSpeed;

    [Header("Detection")]
    public float detectionRange = 8f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointReachDistance = 0.5f;
    private int currentWaypointIndex = 0;

    [Header("Damage")]
    public float damageInterval = 1f;
    private float damageTimer = 0f;

    [Header("Animation")]
    // State ใน Animator เหมือน Player
    // 1=Up 2=Down 3=Left 4=Right, Idle=10/20/30/40

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State
    private PlayerMiniGame4 targetPlayer;
    private Vector2 movement;
    private int lastDirectionState = 2;
    private bool isChasing = false;

    // ─────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentSpeed = moveSpeed;

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyAI] '{gameObject.name}' ไม่มี Patrol Points!");
        }
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (GameManager.isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        DetectPlayer();
        DecideMovement();
        UpdateAnimationState();

        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (GameManager.isPaused) return;
        Move();
    }

    // ─────────────────────────────────────────
    void DetectPlayer()
    {
        // หา Player ในระยะ
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, detectionRange, playerLayer
        );

        targetPlayer = null;

        foreach (Collider2D hit in hits)
        {
            PlayerMiniGame4 player = hit.GetComponent<PlayerMiniGame4>();
            if (player == null) continue;
            if (player.IsHiding()) continue; // ซ่อนอยู่ = มองไม่เห็น

            // เช็คว่ามีกำแพงบังหรือไม่
            Vector2 dirToPlayer = (hit.transform.position - transform.position).normalized;
            float distToPlayer = Vector2.Distance(transform.position, hit.transform.position);

            RaycastHit2D wallCheck = Physics2D.Raycast(
                transform.position, dirToPlayer, distToPlayer, obstacleLayer
            );

            if (wallCheck.collider == null)
            {
                targetPlayer = player;
                break;
            }
        }

        isChasing = (targetPlayer != null);
        currentSpeed = isChasing ? chaseSpeed : moveSpeed;
    }

    // ─────────────────────────────────────────
    void DecideMovement()
    {
        if (isChasing && targetPlayer != null)
        {
            // ไล่ตาม Player
            Vector2 dirToPlayer = (targetPlayer.GetPosition() - rb.position).normalized;
            movement = dirToPlayer;
        }
        else
        {
            // Patrol ตามจุด
            if (patrolPoints.Length > 0)
            {
                Vector2 targetPos = patrolPoints[currentWaypointIndex].position;
                Vector2 dirToWaypoint = (targetPos - rb.position).normalized;
                movement = dirToWaypoint;

                float distToWaypoint = Vector2.Distance(rb.position, targetPos);
                if (distToWaypoint < waypointReachDistance)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
                }
            }
            else
            {
                movement = Vector2.zero;
            }
        }
    }

    // ─────────────────────────────────────────
    void Move()
    {
        rb.linearVelocity = movement * currentSpeed;
    }

    // ─────────────────────────────────────────
    void UpdateAnimationState()
    {
        if (animator == null) return;

        if (movement.magnitude > 0.1f)
        {
            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
                lastDirectionState = movement.x > 0 ? 4 : 3;
            else
                lastDirectionState = movement.y > 0 ? 1 : 2;

            animator.SetInteger("State", lastDirectionState);
        }
        else
        {
            animator.SetInteger("State", lastDirectionState * 10);
        }
    }

    // ─────────────────────────────────────────
    void OnCollisionStay2D(Collision2D collision)
    {
        if (damageTimer > 0f) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMiniGame4 player = collision.gameObject.GetComponent<PlayerMiniGame4>();
            if (player != null && !player.IsHiding())
            {
                player.TakeDamage();
                damageTimer = damageInterval;
                Debug.Log("[EnemyAI] Hit Player!");
            }
        }
    }

    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Patrol path
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                Vector3 current = patrolPoints[i].position;
                Vector3 next = patrolPoints[(i + 1) % patrolPoints.Length].position;

                Gizmos.DrawLine(current, next);
                Gizmos.DrawWireSphere(current, 0.3f);
            }
        }
    }
}