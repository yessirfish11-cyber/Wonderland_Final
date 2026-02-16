using UnityEngine;

public enum InteractType
{
    Hide,   // ซ่อนตัว
    Win     // ชนะ
}

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Type")]
    public InteractType interactType = InteractType.Hide;

    [Header("Visual Hint (optional)")]
    public GameObject interactPrompt; // ป้าย [E] ลอยอยู่เหนือ object

    private PlayerMiniGame3 playerInRange = null;
    private bool playerIsHidingHere = false;

    void Update()
    {
        // แสดง/ซ่อน prompt เมื่อผู้เล่นอยู่ใกล้
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange != null);
    }

    public void Interact(PlayerMiniGame3 player)
    {
        switch (interactType)
        {
            case InteractType.Hide:
                HandleHide(player);
                break;

            case InteractType.Win:
                HandleWin();
                break;
        }
    }

    void HandleHide(PlayerMiniGame3 player)
    {
        if (!player.IsHiding())
        {
            // ซ่อนตัว
            player.SetHiding(true);
            player.transform.position = transform.position;
            playerIsHidingHere = true;
            playerInRange = player;
            Debug.Log("Player is hiding!");
        }
        else
        {
            // ออกจากที่ซ่อน
            player.SetHiding(false);
            playerIsHidingHere = false;
            playerInRange = null;
            Debug.Log("Player came out of hiding!");
        }
    }

    void HandleWin()
    {
        Debug.Log("Player reached win object!");
        GameManager.Instance?.AddScore(1);
        UIManager3.Instance?.ShowWinPanel();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = other.GetComponent<PlayerMiniGame3>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerIsHidingHere && playerInRange != null)
            {
                playerInRange.SetHiding(false);
                playerIsHidingHere = false;
            }
            playerInRange = null;
        }
    }
}