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
    private int lastDirectionState = 2;

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
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
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
