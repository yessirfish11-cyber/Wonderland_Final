using UnityEngine;

/// <summary>
/// Camera follows the player only on the X axis.
/// Y axis stays fixed.
/// </summary>
public class CameraFollowMiniGame5 : MonoBehaviour
{
    [Header("Target")]
    public Transform target;               // Assign Player transform

    [Header("Offset")]
    public float offsetX = 0f;             // Horizontal offset from player
    public float smoothSpeed = 5f;         // Smooth follow speed (0 = instant)

    private float fixedY;
    private float fixedZ;

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offsetX;

        if (smoothSpeed <= 0f)
        {
            // Instant snap
            transform.position = new Vector3(targetX, fixedY, fixedZ);
        }
        else
        {
            // Smooth follow
            float smoothedX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(smoothedX, fixedY, fixedZ);
        }
    }
}