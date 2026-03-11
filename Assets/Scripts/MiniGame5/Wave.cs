using UnityEngine;

/// <summary>
/// The Wave enemy that sweeps from left to right.
/// Spawned by WaveManager after the warning signal.
/// </summary>
public class Wave : MonoBehaviour
{
    private float speed;
    private float destroyX = 30f; // How far right before destroying self

    public void Init(float waveSpeed, float startX, float startY, float endX)
    {
        speed = waveSpeed;
        destroyX = endX;

        // Position wave off-screen to the left
        transform.position = new Vector3(startX, startY, 0f);
    }

    void Update()
    {
        // Move right
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Destroy when off-screen right
        if (transform.position.x > destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerMiniGame5 player = other.GetComponent<PlayerControllerMiniGame5>();
        if (player != null)
        {
            player.TakeDamage();
        }
    }
}