using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    [Header("Map Boundary Settings")]
    public float minX; // ขอบซ้ายสุด
    public float maxX; // ขอบขวาสุด
    public float minY; // ขอบล่างสุด
    public float maxY; // ขอบบนสุด

    void LateUpdate()
    {
        // 1. ดึงตำแหน่งปัจจุบันของตัวละครมาเก็บไว้ในตัวแปรชั่วคราว
        Vector3 viewPos = transform.position;

        // 2. ใช้ Mathf.Clamp เพื่อจำกัดค่าให้อยู่ในช่วงที่กำหนด
        // Mathf.Clamp(ค่าที่จะเช็ค, ค่าต่ำสุด, ค่าสูงสุด)
        viewPos.x = Mathf.Clamp(viewPos.x, minX, maxX);
        viewPos.y = Mathf.Clamp(viewPos.y, minY, maxY);

        // 3. ส่งค่าที่ถูกจำกัดแล้วกลับไปยัง Transform ของตัวละคร
        transform.position = viewPos;
    }
}
