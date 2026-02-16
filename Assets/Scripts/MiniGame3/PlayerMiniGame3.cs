using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PlayerMiniGame3 : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;

    [Header("Health")]
    public int maxLives = 3;
    private int currentLives;
    private bool isInvincible = false;
    public float invincibleDuration = 1f;

    [Header("Interaction")]
    public float interactRange = 1.5f;
    public LayerMask interactableLayer;

    [Header("Animation")]
    // State ที่ใช้ใน Animator:
    // เดิน:  1=Up  2=Down  3=Left  4=Right
    // Idle: 10=Up 20=Down 30=Left 40=Right

    // Private refs
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Animation state
    private int lastDirectionState = 2; // Default: Idle Down
    private Vector2 movement;

    // Game state
    private bool isHiding = false;

    // ─────────────────────────────────────────
    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator     = GetComponent<Animator>();
    }

    void Start()
    {
        currentLives = maxLives;
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (GameManager.isPaused) return;

        ReadInput();
        HandleInteraction();
        UpdateAnimationState();
    }

    void FixedUpdate()
    {
        if (GameManager.isPaused) return;
        HandleMovement();
    }

    // ─────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────
    void ReadInput()
    {
        if (isHiding)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal"); // A/D หรือ Arrow
        movement.y = Input.GetAxisRaw("Vertical");   // W/S หรือ Arrow
        movement = movement.normalized;
    }

    // ─────────────────────────────────────────
    // MOVEMENT (FixedUpdate)
    // ─────────────────────────────────────────
    void HandleMovement()
    {
        if (isHiding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movement * speed;
    }

    // ─────────────────────────────────────────
    // ANIMATION
    // ─────────────────────────────────────────
    void UpdateAnimationState()
    {
        if (animator == null) return;

        if (movement != Vector2.zero)
        {
            // คำนวณทิศทางหลัก
            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
                lastDirectionState = movement.x > 0 ? 4 : 3; // Right=4, Left=3
            else
                lastDirectionState = movement.y > 0 ? 1 : 2; // Up=1, Down=2

            // Walk state
            animator.SetInteger("State", lastDirectionState);
        }
        else
        {
            // Idle state = lastDirection * 10
            animator.SetInteger("State", lastDirectionState * 10);
        }
    }

    // ─────────────────────────────────────────
    // INTERACTION
    // ─────────────────────────────────────────
    void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(
            transform.position, interactRange, interactableLayer
        );

        if (nearbyObjects.Length == 0) return;

        // หา object ที่ใกล้ที่สุด
        Collider2D closest = null;
        float closestDist = Mathf.Infinity;
        foreach (Collider2D col in nearbyObjects)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = col;
            }
        }

        if (closest != null)
        {
            InteractableObject interactable = closest.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.Interact(this);
        }
    }

    // ─────────────────────────────────────────
    // DAMAGE & DEATH
    // ─────────────────────────────────────────
    public void TakeDamage()
    {
        if (isInvincible || isHiding) return;

        currentLives--;
        Debug.Log("Player HP: " + currentLives);

        if (currentLives <= 0)
            Die();
        else
            StartCoroutine(InvincibilityCoroutine());
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        rb.linearVelocity = Vector2.zero;
        UIManager3.Instance?.ShowLosePanel();
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    // HIDING
    // ─────────────────────────────────────────
    public void SetHiding(bool hiding)
    {
        isHiding = hiding;

        if (isHiding)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);

            // เล่น Idle animation ทิศทางสุดท้ายขณะซ่อน
            if (animator != null)
                animator.SetInteger("State", lastDirectionState * 10);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    // ─────────────────────────────────────────
    // PUBLIC GETTERS (ให้ Enemy ใช้เช็ค)
    // ─────────────────────────────────────────
    public bool IsHiding()     => isHiding;
    public bool IsMoving()     => movement.magnitude > 0f;
    public bool IsInvincible() => isInvincible;

    // ─────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}