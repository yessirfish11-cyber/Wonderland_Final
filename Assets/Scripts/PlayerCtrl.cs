using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private PlayerCtrls PlayerCtrls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private int lastDirectionState = 2;

    private bool isSprinting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (PlayerCtrls == null)
        {
            PlayerCtrls = new PlayerCtrls();
        }

        PlayerCtrls.Enable();
    }

    private void Update()
    {
        PlayerInput();
        UpdateAnimationTransitions();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void PlayerInput()
    {
        movement = PlayerCtrls.Movement.Move.ReadValue<Vector2>();
        isSprinting = PlayerCtrls.Movement.Sprint.ReadValue<float>() > 0;
    }

    private void Move()
    {
        if (movement == Vector2.zero) return;

        // เลือกความเร็ว
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // ส่วนสำหรับ Debug (ลบออกได้เมื่อใช้งานได้แล้ว)
        // ถ้าวิ่งอยู่จะขึ้นข้อความใน Console ว่า "Sprinting!"
        if (isSprinting) Debug.Log("กำลังวิ่งด้วยความเร็ว: " + currentSpeed);

        // คำนวณการเคลื่อนที่
        Vector2 moveAmount = movement * (currentSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + moveAmount);
    }

    private void UpdateAnimationTransitions()
    {
        if (movement != Vector2.zero)
        {
            // 1. คำนวณหาทิศทางที่กด และอัปเดต lastDirectionState
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
            // 2. ถ้าหยุดเดิน ให้เช็คว่าทิศทางล่าสุดคืออะไร แล้วส่งค่า Idle ของทิศนั้นไป
            // เราจะกำหนดค่า Idle ให้เป็นเลขลบหรือเลขหลักสิบก็ได้ครับ 
            // ในที่นี้ผมขอใช้: 10=IdleW, 20=IdleS, 30=IdleA, 40=IdleD
            animator.SetInteger("State", lastDirectionState * 10);
        }
    }
}
