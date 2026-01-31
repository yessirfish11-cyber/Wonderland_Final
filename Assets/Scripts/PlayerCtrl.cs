using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private PlayerCtrls PlayerCtrls;
    private Vector2 movement;
    private Rigidbody2D rb;

    private bool isSprinting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
}
