using UnityEngine;

public class ArhiaPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform[] waypoints; // ใส่จุดกี่จุดก็ได้ใน Inspector (0, 1, 2, 3...)
    public float moveSpeed = 2f;

    private Animator anim;
    private int currentPointIndex = 0;

    void Start()
    {
        anim = GetComponent<Animator>();

        // เริ่มต้นที่จุดแรกที่กำหนดไว้
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        // ต้องมีอย่างน้อย 2 จุดเพื่อให้เดินไปมาได้
        if (waypoints.Length < 2) return;

        MoveNPC();
    }

    void MoveNPC()
    {
        Transform target = waypoints[currentPointIndex];

        // 1. คำนวณทิศทางแกน X เพื่อหาว่าจะหันซ้ายหรือขวา
        float directionX = target.position.x - transform.position.x;

        // 2. ส่งค่าไปที่ Animator (เช็คแค่ซ้ายหรือขวา)
        if (directionX > 0.05f)
        {
            // ถ้าเป้าหมายอยู่ทางขวามากกว่าตัวเรา ให้หันขวา
            anim.SetBool("isFacingRight", true);
        }
        else if (directionX < -0.05f)
        {
            // ถ้าเป้าหมายอยู่ทางซ้ายมากกว่าตัวเรา ให้หันซ้าย
            anim.SetBool("isFacingRight", false);
        }
        // หมายเหตุ: ถ้าค่า directionX ใกล้ 0 มาก (เดินขึ้นลงตรงๆ) 
        // มันจะคงท่าหันเดิมเอาไว้ ไม่เปลี่ยนฝั่งกระทันหัน

        // 3. สั่งให้เคลื่อนที่ไปยังจุดหมาย (รองรับทั้ง X และ Y)
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // 4. ตรวจสอบระยะห่างว่าถึงจุดหมายหรือยัง
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            // เปลี่ยนไปยังจุดถัดไปในลิสต์ (0 -> 1 -> 2 -> 0)
            currentPointIndex = (currentPointIndex + 1) % waypoints.Length;
        }
    }
}
