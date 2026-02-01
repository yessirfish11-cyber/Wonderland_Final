using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerSpeed;
    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 playerDirection;

    private int currentHealth = 4;
    private int maxHealth = 4;
    
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
        currentHealth -= damage;
        ApplySpeedDebuff(speedReduction, slowDuration);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplySpeedDebuff(float speedReduction, float duration)
    {
        if (isSlowed)
        {
            currentSpeed = originalSpeed;
        }
        
        currentSpeed = originalSpeed - speedReduction;
        
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

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}