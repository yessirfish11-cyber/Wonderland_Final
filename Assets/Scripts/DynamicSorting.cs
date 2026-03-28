using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private int offset = 0; // ปรับเพิ่ม/ลดลำดับได้ถ้าต้องการ
    [SerializeField] private bool isStatic = false; // ถ้าเป็นวัตถุอยู่นิ่งๆ เช่น รั้ว ให้ติ๊กถูกอันนี้

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ถ้าเป็นวัตถุที่ไมขยับ ให้คำนวณแค่ครั้งเดียวตอนเริ่มเกมเพื่อประหยัดทรัพยากร
        if (isStatic)
        {
            UpdateSortingOrder();
            enabled = false; // ปิดสคริปต์นี้ไปเลยหลังจากคำนวณเสร็จ
        }
    }

    void LateUpdate()
    {
        UpdateSortingOrder();
    }

    void UpdateSortingOrder()
    {
        // สูตร: เอาตำแหน่ง Y มาคูณ -100 (เพื่อให้ค่า Y ต่ำ กลายเป็นเลข Order ที่สูง)
        // เช่น Y = -1.5 จะได้ Order = 150
        // เช่น Y = -1.2 จะได้ Order = 120 (ตัวที่อยู่ 150 จะอยู่หน้า 120)
        spriteRenderer.sortingOrder = (int)(transform.position.y * -100) + offset;
    }
}
