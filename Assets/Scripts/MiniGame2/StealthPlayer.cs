using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StealthPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;

    [Header("Light Settings")]
    [SerializeField] private Light2D playerLight;
    [SerializeField] private float minLightIntensity = 0.2f;
    [SerializeField] private float maxLightIntensity = 1.5f;
    [SerializeField] private float minLightRadius = 1.5f;
    [SerializeField] private float maxLightRadius = 6f;

    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private int lastDirectionState = 2;
    private bool isSprinting;
    private bool canMove = true;

    // Properties สำหรับ AI ศัตรูเช็ค
    public bool IsSprinting => isSprinting;
    public bool IsMoving => movement.magnitude > 0f;
    public Vector2 Position => rb.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (playerLight == null)
        {
            playerLight = GetComponentInChildren<Light2D>();
        }
    }

    private void Update()
    {
        if (canMove)
        {
            PlayerInput();
            UpdateAnimationTransitions();
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            Move();
        }
    }

    private void PlayerInput()
    {
        // WASD input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // Left Shift สำหรับวิ่ง
        isSprinting = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0;
    }

    private void Move()
    {
        if (movement == Vector2.zero) return;

        // เลือกความเร็ว
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // คำนวณการเคลื่อนที่
        Vector2 moveAmount = movement * (currentSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + moveAmount);
    }

    private void UpdateAnimationTransitions()
    {
        if (animator == null) return;

        if (movement != Vector2.zero)
        {
            // คำนวณหาทิศทางที่กด และอัปเดต lastDirectionState
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirectionState = movement.x > 0 ? 4 : 3; // ขวา หรือ ซ้าย
            }
            else
            {
                lastDirectionState = movement.y > 0 ? 1 : 2; // บน หรือ ล่าง
            }

            // ส่งค่า State ของการเดิน (1-4) ไปที่ Animator
            animator.SetInteger("State", lastDirectionState);
        }
        else
        {
            // ถ้าหยุดเดิน ให้ส่งค่า Idle
            // 10=IdleW, 20=IdleS, 30=IdleA, 40=IdleD
            animator.SetInteger("State", lastDirectionState * 10);
        }
    }

    public void SetMovement(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }
}