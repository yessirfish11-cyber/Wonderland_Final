using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private HashSet<GameObject> portalObjects = new HashSet<GameObject>(); // เก็บวัตถุที่เป็น Portal เพื่อป้องกันการวาร์ปซ้ำ

    [SerializeField] public Transform destination; // ลากจุดที่ต้องการให้วาร์ปไปมาใส่ตรงนี้
    [SerializeField] private float warpDelay = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (portalObjects.Contains(collision.gameObject)) return;

            // เริ่มนับเวลาถอยหลังก่อนวาร์ป
            StartCoroutine(WarpRoutine(collision.gameObject));
        }
    }

    private IEnumerator WarpRoutine(GameObject player)
    {
        // 1. รอตามเวลาที่กำหนด (เช่น 0.5 วินาที)
        yield return new WaitForSeconds(warpDelay);

        // 2. เช็คอีกครั้งว่าผู้เล่นยังอยู่ใน Trigger หรือไม่ (ป้องกันกรณีเดินผ่านไปเลย)
        // ถ้าต้องการให้เหยียบปุ๊บวาร์ปปั๊บแต่แค่ให้ดูช้าลง ให้ข้ามเช็คนี้ไปได้ครับ

        if (destination.TryGetComponent(out Teleporter destinationPortal))
        {
            destinationPortal.portalObjects.Add(player);
        }

        // 3. ทำการวาร์ป
        player.transform.position = destination.position;

        Debug.Log("Teleported after delay!");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // เมื่อออกจากพื้นที่ ให้ลบออกจากรายการป้องกันวาร์ปซ้ำ
        // และหยุด Coroutine หากผู้เล่นเดินออกจากจุดวาร์ปก่อนเวลาจะครบ (ถ้าต้องการ)
        StopAllCoroutines();
        portalObjects.Remove(collision.gameObject);
    }
}
