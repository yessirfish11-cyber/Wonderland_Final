using UnityEngine;

public class ObjHits : MonoBehaviour
{
    private GameObject player;
    
    // Enemy Type Configuration
    public enum EnemyType
    {
        EnemyA,  // -4 speed, 2 cooldown, -2 health
        EnemyB,  // -3 speed, 2 cooldown, -1 health
        EnemyC   // -1 speed, 3 cooldown, no damage
    }
    
    public EnemyType enemyType = EnemyType.EnemyA;

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
            Player playerScript = collision.GetComponent<Player>();
            
            if (playerScript != null)
            {
                // Apply different effects based on enemy type
                switch (enemyType)
                {
                    case EnemyType.EnemyA:
                        playerScript.TakeDamage(2, 2f, 2f); // 2 damage, -4 speed, 2 sec cooldown
                        break;
                        
                    case EnemyType.EnemyB:
                        playerScript.TakeDamage(1, 3f, 2f); // 1 damage, -3 speed, 2 sec cooldown
                        break;
                        
                    case EnemyType.EnemyC:
                        playerScript.TakeDamage(0, 4f, 2f); // 0 damage, -1 speed, 3 sec cooldown
                        break;
                }
            }
        
            Destroy(this.gameObject);
        }
    }
}