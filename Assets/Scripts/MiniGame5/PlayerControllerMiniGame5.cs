using System.Collections;
using UnityEngine;

public class PlayerControllerMiniGame5 : MonoBehaviour
{
    [Header("Movement")]
    public float autoMoveSpeed = 5f;
    public float verticalSpeed = 5f;

    [Header("State")]
    public bool isHidden = false;
    public int maxHP = 3;
    private int currentHP;

    [Header("Invincibility")]
    public float invincibleDuration = 2f;   // วินาทีที่อมตะหลังโดนโจมตี
    public float blinkRate = 0.1f;          // ความเร็วกระพริบ (วินาทีต่อครั้ง)
    private bool isInvincible = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    // Reference to current interactable in range
    private IInteractableMiniGame5 nearbyInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
    }

    void HandleMovement()
    {
        // หยุดทุกทิศทางเมื่อซ่อนอยู่
        if (isHidden)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        else if (Input.GetKey(KeyCode.S)) vertical = -1f;

        // Always move right automatically
        float horizontal = 1f;

        rb.linearVelocity = new Vector2(horizontal * autoMoveSpeed, vertical * verticalSpeed);
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && nearbyInteractable != null)
        {
            nearbyInteractable.Interact(this);
        }
    }

    public void SetNearbyInteractable(IInteractableMiniGame5 interactable)
    {
        nearbyInteractable = interactable;
    }

    public void ClearNearbyInteractable(IInteractableMiniGame5 interactable)
    {
        if (nearbyInteractable == interactable)
            nearbyInteractable = null;
    }

    public void TakeDamage()
    {
        // ไม่รับ Damage ถ้าซ่อนอยู่ หรือกำลังอมตะ
        if (isHidden || isInvincible) return;

        currentHP--;
        Debug.Log($"Player HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // เริ่ม Invincibility หลังโดนโจมตี
            StartCoroutine(InvincibilityRoutine());
        }
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        // กระพริบตลอดช่วงอมตะ
        while (elapsed < invincibleDuration)
        {
            SetSpriteAlpha(0.2f);
            yield return new WaitForSeconds(blinkRate);

            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(blinkRate);

            elapsed += blinkRate * 2f;
        }

        // จบ Invincibility — คืนค่า Alpha ให้ปกติ
        SetSpriteAlpha(1f);
        isInvincible = false;
        Debug.Log("Invincibility ended");
    }

    void SetSpriteAlpha(float alpha)
    {
        if (sr == null) return;
        // ถ้าซ่อนอยู่ ไม่ต้องเปลี่ยน Alpha (ให้ Hide() จัดการ)
        if (isHidden) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // หยุด Coroutine ทั้งหมดก่อน Reload
        StopAllCoroutines();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void Hide()
    {
        isHidden = true;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.3f;
            sr.color = c;
        }
        Debug.Log("Player is HIDDEN");
    }

    public void Unhide()
    {
        isHidden = false;
        if (sr != null)
        {
            Color c = sr.color;
            // ถ้ายังอมตะอยู่ ให้คืน alpha ครึ่งๆ ก่อน จน Coroutine จัดการเอง
            c.a = isInvincible ? 0.5f : 1f;
            sr.color = c;
        }
        Debug.Log("Player is VISIBLE");
    }
}