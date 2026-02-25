using UnityEngine;

public enum InteractTypeMG4
{
    Hide,          // ซ่อนตัว (กด E)
    Collect,       // เก็บไอเทมปกติ (เดินชน)
    CollectFinal   // เก็บไอเทมสุดท้าย (เดินชน = ชนะเลย!)
}

public class InteractableObjectMG4 : MonoBehaviour
{
    [Header("Interaction Type")]
    public InteractTypeMG4 interactType = InteractTypeMG4.Collect;

    [Header("Visual Hint")]
    public GameObject interactPrompt;

    [Header("Collect Settings")]
    public bool destroyAfterCollect = true;

    private PlayerMiniGame4 playerInRange = null;
    private bool playerIsHidingHere = false;
    private bool isCollected = false;

    void Update()
    {
        if (interactPrompt != null)
        {
            bool shouldShow = (interactType == InteractTypeMG4.Hide) && 
                              (playerInRange != null);
            interactPrompt.SetActive(shouldShow);
        }
    }

    public void Interact(PlayerMiniGame4 player)
    {
        if (interactType == InteractTypeMG4.Hide)
        {
            HandleHide(player);
        }
    }

    void HandleHide(PlayerMiniGame4 player)
    {
        if (!player.IsHiding())
        {
            player.SetHiding(true);
            player.transform.position = transform.position;
            playerIsHidingHere = true;
            playerInRange = player;
            Debug.Log($"[Interact] Player hiding in '{gameObject.name}'");
        }
        else
        {
            player.SetHiding(false);
            playerIsHidingHere = false;
            playerInRange = null;
            Debug.Log($"[Interact] Player came out from '{gameObject.name}'");
        }
    }

    void HandleCollect(PlayerMiniGame4 player)
    {
        if (isCollected) return;

        isCollected = true;

        // เช็คว่าเป็น Final Item หรือไม่
        bool isFinalItem = (interactType == InteractTypeMG4.CollectFinal);
        
        player.CollectItem(isFinalItem);
        
        if (isFinalItem)
            Debug.Log($"[Interact] 🎉 Final item '{gameObject.name}' collected! WIN!");
        else
            Debug.Log($"[Interact] Item '{gameObject.name}' collected!");

        if (destroyAfterCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMiniGame4 player = other.GetComponent<PlayerMiniGame4>();
        if (player == null) return;

        playerInRange = player;

        // เก็บไอเทมทั้งแบบปกติและ Final
        if (interactType == InteractTypeMG4.Collect || 
            interactType == InteractTypeMG4.CollectFinal)
        {
            HandleCollect(player);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (interactType == InteractTypeMG4.Hide)
        {
            if (playerIsHidingHere && playerInRange != null)
            {
                playerInRange.SetHiding(false);
                playerIsHidingHere = false;
            }
        }

        playerInRange = null;
    }

    public bool IsPlayerHidingHere()
    {
        return playerIsHidingHere;
    }

    public void ForcePlayerOut()
    {
        if (playerIsHidingHere && playerInRange != null)
        {
            playerInRange.SetHiding(false);
            playerIsHidingHere = false;
            Debug.Log($"[Interact] '{gameObject.name}' destroyed! Player forced out!");
        }
    }

    void OnDestroy()
    {
        if (playerIsHidingHere && playerInRange != null)
        {
            playerInRange.SetHiding(false);
        }
    }
}