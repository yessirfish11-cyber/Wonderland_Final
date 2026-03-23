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
        // แม้ movement เป็น zero ก็ควรให้ rb.velocity เป็น zero เพื่อป้องกันแรงเฉื่อยค้าง
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // ใช้ velocity หรือ MovePosition ก็ได้ แต่แนะนำให้คูณ currentSpeed เข้าไปตรงๆ
        rb.linearVelocity = movement * currentSpeed;
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
