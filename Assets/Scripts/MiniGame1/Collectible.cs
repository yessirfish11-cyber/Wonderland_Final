using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int scoreValue = 1;
    
    // Optional: Add sound effect
    public AudioClip collectSound;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            // Add score to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }
            
            // Optional: Play sound effect
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Destroy the collectible
            Destroy(gameObject);
        }
    }
}