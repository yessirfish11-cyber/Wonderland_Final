using UnityEngine;

public class ObjHits : MonoBehaviour
{
    private GameObject player;
    
    public enum EnemyType
    {
        EnemyA,  
        EnemyB,  
        EnemyC,
        EnemyD   // ชนแล้วตายทันที
    }
    
    public EnemyType enemyType = EnemyType.EnemyA;

    [Header("Hit Sound Effects")]
    public AudioClip enemyAHitSound;
    public AudioClip enemyBHitSound;
    public AudioClip enemyCHitSound;
    public AudioClip enemyDHitSound;

    [Range(0f, 1f)] public float hitVolume = 1f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
        else if(collision.tag == "Player")
        {
            Player1 playerScript = collision.GetComponent<Player1>();
            
            if (playerScript != null)
            {
                switch (enemyType)
                {
                    case EnemyType.EnemyA:
                        playerScript.TakeDamage(2, 2f, 2f);
                        PlayHitSound(enemyAHitSound);
                        break;
                        
                    case EnemyType.EnemyB:
                        playerScript.TakeDamage(1, 3f, 2f);
                        PlayHitSound(enemyBHitSound);
                        break;
                        
                    case EnemyType.EnemyC:
                        playerScript.TakeDamage(0, 4f, 2f);
                        PlayHitSound(enemyCHitSound);
                        break;

                    case EnemyType.EnemyD:
                        playerScript.TakeDamage(playerScript.GetCurrentHealth(), 0f, 0f);
                        PlayHitSound(enemyDHitSound);
                        break;
                }
            }
        
            Destroy(this.gameObject);
        }
    }

    private void PlayHitSound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, hitVolume);
        }
    }
}