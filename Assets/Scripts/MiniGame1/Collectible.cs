using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int scoreValue = 1;
    public AudioClip collectSound;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }
            
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
}