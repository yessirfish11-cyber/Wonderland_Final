using UnityEngine;

/// <summary>
/// Attach to the goal object (e.g., exit door, treasure chest).
/// When player presses E near this, trigger WIN condition.
/// </summary>
public class WinObjectMiniGame5 : MonoBehaviour, IInteractableMiniGame5
{
    public void Interact(PlayerControllerMiniGame5 player)
    {
        Debug.Log("Player reached the WIN object!");
        GameManagerMiniGame5.Instance.TriggerWin();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerMiniGame5 player = other.GetComponent<PlayerControllerMiniGame5>();
        if (player != null)
        {
            player.SetNearbyInteractable(this);
            Debug.Log("[E] to Win!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerControllerMiniGame5 player = other.GetComponent<PlayerControllerMiniGame5>();
        if (player != null)
        {
            player.ClearNearbyInteractable(this);
        }
    }
}