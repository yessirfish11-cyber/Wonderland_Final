using UnityEngine;

public enum InteractType
{
    Hide,
    Win
}

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Type")]
    public InteractType interactType = InteractType.Hide;

    [Header("Visual Hint (optional)")]
    public GameObject interactPrompt;

    [Header("Sound")]
    public AudioClip hideSound;

    private AudioSource audioSource;
    private PlayerMiniGame3 playerInRange = null;
    private bool playerIsHidingHere = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange != null);

        // ❌ ลบ Input.GetKeyDown(KeyCode.E) ออกจากที่นี่
        // PlayerMiniGame3.HandleInteraction() จัดการอยู่แล้ว
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
            player.SetHiding(true);
            player.transform.position = transform.position;
            playerIsHidingHere = true;
            playerInRange = player;

            if (hideSound != null)
                audioSource.PlayOneShot(hideSound);

            Debug.Log("Player is hiding!");
        }
        else
        {
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