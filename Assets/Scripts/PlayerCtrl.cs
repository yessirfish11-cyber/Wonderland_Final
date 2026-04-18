using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private PlayerCtrls PlayerCtrls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    public int lastDirectionState = 2;

    private bool isSprinting;

    [Header("Audio Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;   // ความถี่เสียงตอนเดิน
    [SerializeField] private float sprintStepInterval = 0.3f; // ความถี่เสียงตอนวิ่ง (เร็วขึ้น)
    [SerializeField] private AudioClip footstepClip;         // ไฟล์เสียงเท้า

    private float stepTimer; // ตัวนับเวลา


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        PlayerCtrls = new PlayerCtrls();    


    }

    private void OnEnable()
    {
       
            PlayerCtrls.Enable();
     
    }

    private void OnDisable()
    {
        
            PlayerCtrls.Disable();
        
    }
            // ปิดการใช้งานเมื่อเปลี่ยนฉากหรือ Object ถูกทำลาย
        

    private void Update()
    {
        PlayerInput();
        UpdateAnimationTransitions();
        HandleFootstepSound();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void PlayerInput()
    {
        // ตรวจสอบโครงสร้าง Actions ใน Input Action Asset ของคุณ 
        // (สมมติว่าตั้งชื่อ Action Map ว่า "Movement" และ Action ว่า "Move" และ "Sprint")
        movement = PlayerCtrls.Movement.Move.ReadValue<Vector2>();
        isSprinting = PlayerCtrls.Movement.Sprint.ReadValue<float>() > 0;
    }

    private void Move()
    {
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // ใช้ rb.velocity เพื่อความชัวร์ (รองรับทุกเวอร์ชัน)
        rb.velocity = movement * currentSpeed;
    }

    private void HandleFootstepSound()
    {
        // เงื่อนไข: ถ้ามีการกดเคลื่อนที่ (movement ไม่เป็น 0)
        if (movement.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                // ปรับจังหวะก้าวตามสถานะ เดิน/วิ่ง
                stepTimer = isSprinting ? sprintStepInterval : walkStepInterval;
            }
        }
        else
        {
            // ถ้าหยุดอยู่กับที่ ให้รีเซ็ต Timer 
            // เพื่อให้ก้าวแรกที่เดินใหม่มีเสียงทันที และเสียงเก่าไม่ค้าง
            stepTimer = 0f;
        }
    }

    private void PlayFootstepSound()
    {
        if (AudioManager.Instance != null && footstepClip != null)
        {
            // เล่นเสียงผ่าน SFX Source
            AudioManager.Instance.sfxSource.PlayOneShot(footstepClip);
        }
    }

    private void UpdateAnimationTransitions()
    {
        if (movement != Vector2.zero)
        {
            // หาค่าทิศทาง 1, 2, 3, 4 เหมือนเดิม
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirectionState = movement.x > 0 ? 4 : 3;
            }
            else
            {
                lastDirectionState = movement.y > 0 ? 1 : 2;
            }

            // --- เพิ่มส่วนนี้ ---
            if (isSprinting)
            {
                // ถ้าวิ่ง ให้ส่งค่าเป็นหลักร้อย (100, 200, 300, 400)
                animator.SetInteger("State", lastDirectionState * 100);
            }
            else
            {
                // ถ้าเดินปกติ ให้ส่งค่า 1, 2, 3, 4
                animator.SetInteger("State", lastDirectionState);
            }
        }
        else
        {
            // ถ้าหยุดเดิน ส่งค่า Idle 10, 20, 30, 40
            animator.SetInteger("State", lastDirectionState * 10);
        }
    }

    public int GetLastDirection()
    {
        return lastDirectionState;
    }
}
