using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    public string sceneToLoad;
    private bool isReady = false; // เริ่มต้นจะวาร์ปไม่ได้

    // NPC จะเรียกฟังก์ชันนี้เมื่อคุยจบ
    public void UnlockTrigger()
    {
        isReady = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้าเป็นผู้เล่นเดินมาชน และ NPC ปลดล็อกให้แล้ว
        if (isReady && other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
