using TMPro;
using UnityEngine;

public class ChoiceTrigger : MonoBehaviour
{
    public GameObject boatSelectionPanel; // ลาก BoatSelectionPanel มาใส่
    public GameObject interactPrompt;
    private bool playerIsInside = false;

    void Update()
    {
        // ถ้าคุยจบแล้ว และกด E ที่กองไม้
        if (playerIsInside && DialogueManager.Instance.isWaitingForSelection && Input.GetKeyDown(KeyCode.E))
        {
            // เปิดหน้า Panel เลือกเรือ 3 ลำ
            boatSelectionPanel.SetActive(true);
            interactPrompt.SetActive(false); // ปิดปุ่ม E
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && DialogueManager.Instance.isWaitingForSelection)
        {
            playerIsInside = true;
            interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
            interactPrompt.SetActive(false);
        }
    }
}
