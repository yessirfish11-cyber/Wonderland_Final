using UnityEngine;

public class ObjHits : MonoBehaviour
{
    private GameObject player;
    
    public enum EnemyType
    {
        EnemyA,  
        EnemyB,  
        EnemyC   
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
            Player1 playerScript = collision.GetComponent<Player1>();
            
            if (playerScript != null)
            {
                switch (enemyType)
                {
                    case EnemyType.EnemyA:
                        playerScript.TakeDamage(2, 2f, 2f); // damage,Speed,cooldown
                        break;
                        
                    case EnemyType.EnemyB:
                        playerScript.TakeDamage(1, 3f, 2f);
                        break;
                        
                    case EnemyType.EnemyC:
                        playerScript.TakeDamage(0, 4f, 2f);
                        break;
                }
            }
        
            Destroy(this.gameObject);
        }
    }
}