using UnityEngine;

/// <summary>
/// Attach to objects that the player can HIDE inside.
/// When player presses E near this object, they become hidden from waves.
/// Press E again to exit hiding.
/// </summary>
public class HideObjectMiniGame5 : MonoBehaviour, IInteractableMiniGame5
{
    private bool playerInsideRange = false;
    private PlayerControllerMiniGame5 playerInside = null;

    public void Interact(PlayerControllerMiniGame5 player)
    {
        if (!player.isHidden)
        {
            // Hide the player
            player.Hide();
            playerInside = player;
        }
        else
        {
            // Unhide the player
            player.Unhide();
            playerInside = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerMiniGame5 player = other.GetComponent<PlayerControllerMiniGame5>();
        if (player != null)
        {
            playerInsideRange = true;
            player.SetNearbyInteractable(this);
            ShowPrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerControllerMiniGame5 player = other.GetComponent<PlayerControllerMiniGame5>();
        if (player != null)
        {
            playerInsideRange = false;

            // Auto-unhide if player leaves the hiding spot
            if (player.isHidden)
            {
                player.Unhide();
                playerInside = null;
            }

            player.ClearNearbyInteractable(this);
            ShowPrompt(false);
        }
    }

    void ShowPrompt(bool show)
    {
        // If you have a UI prompt (optional), toggle it here
        Debug.Log(show ? "[E] Hide" : "");
    }
}