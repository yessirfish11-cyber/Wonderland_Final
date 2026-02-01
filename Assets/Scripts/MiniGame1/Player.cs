using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerSpeed;
    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 playerDirection;
    
    // Health System
    private int currentHealth = 4;
    private int maxHealth = 4;
    
    // Speed Debuff System
    private bool isSlowed = false;
    private float slowEndTime;
    private float originalSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = playerSpeed;
        originalSpeed = playerSpeed;
    }

    void Update()
    {
        // Check if slow debuff has expired
        if (isSlowed && Time.time >= slowEndTime)
        {
            ResetSpeed();
        }

        float directionY = Input.GetAxisRaw("Vertical");
        playerDirection = new Vector2(0, directionY).normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(0, playerDirection.y * currentSpeed);
    }

    public void TakeDamage(int damage, float speedReduction, float slowDuration)
    {
        // Apply damage
        currentHealth -= damage;
        
        // Apply speed reduction
        ApplySpeedDebuff(speedReduction, slowDuration);
        
        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplySpeedDebuff(float speedReduction, float duration)
    {
        // Reset to original speed first if already slowed
        if (isSlowed)
        {
            currentSpeed = originalSpeed;
        }
        
        // Apply new speed reduction
        currentSpeed = originalSpeed - speedReduction;
        
        // Make sure speed doesn't go below 0
        if (currentSpeed < 0)
        {
            currentSpeed = 0;
        }
        
        isSlowed = true;
        slowEndTime = Time.time + duration;
    }

    private void ResetSpeed()
    {
        currentSpeed = originalSpeed;
        isSlowed = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    // Optional: Method to get current health (for debugging)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}