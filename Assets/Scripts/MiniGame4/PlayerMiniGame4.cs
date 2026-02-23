using UnityEngine;
using System.Collections;

public class PlayerMiniGame4 : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 7f;
    private float currentSpeed;

    [Header("Health")]
    public int maxLives = 3;
    private int currentLives;
    private bool isInvincible = false;
    public float invincibleDuration = 1f;

    [Header("Interaction")]
    public float interactRange = 1.5f;
    public LayerMask interactableLayer;

    [Header("Collection")]
    private int itemsCollected = 0;
    public int itemsNeededToWin = 3;

    [Header("Animation")]
    // State: 1=Up 2=Down 3=Left 4=Right, Idle=10/20/30/40

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // State
    private Vector2 movement;
    private int lastDirectionState = 2;
    private bool isHiding = false;
    private bool isSprinting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentLives = maxLives;
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (GameManager.isPaused) return;

        ReadInput();
        HandleInteraction(); // เฉพาะ Hide (กด E)
        UpdateAnimationState();
    }

    void FixedUpdate()
    {
        if (GameManager.isPaused) return;
        HandleMovement();
    }

    void ReadInput()
    {
        if (isHiding)
        {
            movement = Vector2.zero;
            isSprinting = false;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
    }

    void HandleMovement()
    {
        if (isHiding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movement * currentSpeed;
    }

    void UpdateAnimationState()
    {
        if (animator == null) return;

        if (movement != Vector2.zero)
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
    // กด E เฉพาะ Hide type เท่านั้น
    void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(
            transform.position, interactRange, interactableLayer
        );

        if (nearbyObjects.Length == 0) return;

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
            InteractableObjectMG4 interactable = closest.GetComponent<InteractableObjectMG4>();
            if (interactable != null)
            {
                // Interact จะทำงานเฉพาะ Hide type
                // Collect type ถูกเรียกใน OnTriggerEnter2D ของ Object เอง
                interactable.Interact(this);
            }
        }
    }

    // ─────────────────────────────────────────
    public void TakeDamage()
    {
        if (isInvincible || isHiding) return;

        currentLives--;
        Debug.Log($"[Player] HP: {currentLives}/{maxLives}");

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
        Debug.Log("[Player] Died!");
        rb.linearVelocity = Vector2.zero;
        UIManager4.Instance?.ShowLosePanel();
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    public void SetHiding(bool hiding)
    {
        isHiding = hiding;

        if (isHiding)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);

            if (animator != null)
                animator.SetInteger("State", lastDirectionState * 10);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void CollectItem()
    {
        itemsCollected++;
        Debug.Log($"[Player] ✅ Items: {itemsCollected}/{itemsNeededToWin}");

        if (itemsCollected >= itemsNeededToWin)
            Win();
    }

    void Win()
    {
        Debug.Log("[Player] 🎉 WIN!");
        rb.linearVelocity = Vector2.zero;
        UIManager4.Instance?.ShowWinPanel();
    }

    // ─────────────────────────────────────────
    public bool IsHiding() => isHiding;
    public bool IsMoving() => movement.magnitude > 0f;
    public bool IsSprinting() => isSprinting;
    public Vector2 GetPosition() => rb.position;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}