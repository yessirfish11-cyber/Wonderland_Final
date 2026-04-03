using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy ที่มี Vision Cone หมุน 360 องศา + Patrol ระหว่าง Waypoints
/// + เปลี่ยน Sprite ทันทีเมื่อเจอ/ไม่เจอผู้เล่น
/// </summary>
public class EnemyVisionCone : MonoBehaviour
{
    // ─── Patrol ───────────────────────────────
    [System.Serializable]
    public struct PatrolPoint
    {
        public Transform waypoint;   // ลาก Transform ใส่ใน Inspector
        [Min(0f)]
        public float waitTime;       // รอกี่วินาทีที่จุดนี้ก่อนเดินต่อ
    }

    [Header("Patrol Settings")]
    public PatrolPoint[] patrolPoints;   // กำหนด Waypoints ใน Inspector
    public float moveSpeed      = 2f;    // ความเร็วเดิน
    public float waypointRadius = 0.15f; // ระยะที่ถือว่า "ถึง" จุดหมาย

    // ─── Vision Settings ──────────────────────
    [Header("Vision Settings")]
    public float visionAngle    = 60f;
    public float visionRange    = 5f;
    public float rotationSpeed  = 45f;

    [Header("Rotation Mode")]
    public bool rotateClockwise = true;

    [Header("Detection")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Damage")]
    public float damageCooldown = 1.0f;
    private float damageTimer   = 0f;

    [Header("Cone Mesh")]
    public int rayCount = 30;

    [Header("Sound")]
    public AudioClip attackSound;
    private AudioSource audioSource;

    // ─── Components ───────────────────────────
    private Animator    animator;
    private Rigidbody2D rb;
    private GameObject  coneChild;
    private MeshFilter  meshFilter;
    private Mesh        visionMesh;

    // ─── Vision State ─────────────────────────
    private float currentAngle = 0f;
    private int   currentFacingDirection = 2;
    private bool  isAlert          = false;
    private bool  wasAlertLastFrame = false;

    // ─── Patrol State ─────────────────────────
    private enum PatrolState { Patrolling, Waiting, Alert }
    private PatrolState patrolState = PatrolState.Patrolling;

    private int   currentPointIndex = 0;
    private float waitTimer         = 0f;

    // ══════════════════════════════════════════
    void Start()
    {
        animator    = GetComponent<Animator>();
        rb          = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log($"[VisionCone] playerLayer={playerLayer.value} obstacleLayer={obstacleLayer.value}");
        if (playerLayer.value == 0)
            Debug.LogError("[VisionCone] *** playerLayer ว่างอยู่! ตั้งค่าใน Inspector! ***");

        // ตั้งค่า Rigidbody2D ให้ไม่หมุนตามฟิสิกส์
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        CreateConeChildObject();
        UpdateAnimationState();
    }

    // ══════════════════════════════════════════
    void Update()
    {
        if (visionMesh == null) return;

        RotateCone();
        DrawVisionCone();
        UpdateFacingDirection();

        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        CheckPlayerDetection();
        UpdatePatrol();
        UpdateAnimationState();
    }

    // ══════════════════════════════════════════
    //  PATROL
    // ══════════════════════════════════════════

    void UpdatePatrol()
    {
        // ถ้า Alert — หยุดเดิน
        if (isAlert)
        {
            patrolState = PatrolState.Alert;
            StopMovement();
            return;
        }

        // กลับจาก Alert → เริ่ม Patrol ต่อ
        if (patrolState == PatrolState.Alert)
            patrolState = PatrolState.Patrolling;

        // ไม่มี waypoint — ไม่ต้องทำอะไร
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        switch (patrolState)
        {
            case PatrolState.Patrolling:
                MoveTowardsCurrentPoint();
                break;

            case PatrolState.Waiting:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    AdvanceToNextPoint();
                    patrolState = PatrolState.Patrolling;
                }
                break;
        }
    }

    void MoveTowardsCurrentPoint()
    {
        if (patrolPoints[currentPointIndex].waypoint == null) return;

        Vector2 target    = patrolPoints[currentPointIndex].waypoint.position;
        Vector2 current   = transform.position;
        Vector2 direction = (target - current).normalized;
        float   distance  = Vector2.Distance(current, target);

        if (distance <= waypointRadius)
        {
            // ถึงจุดแล้ว
            StopMovement();
            float wait = patrolPoints[currentPointIndex].waitTime;
            if (wait > 0f)
            {
                waitTimer   = wait;
                patrolState = PatrolState.Waiting;
            }
            else
            {
                AdvanceToNextPoint();
            }
            return;
        }

        // เดิน
        if (rb != null)
            rb.linearVelocity = direction * moveSpeed;
        else
            transform.position = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
    }

    void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void AdvanceToNextPoint()
    {
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    // ══════════════════════════════════════════
    //  VISION CONE
    // ══════════════════════════════════════════

    void CreateConeChildObject()
    {
        Transform old = transform.Find("VisionConeMesh");
        if (old != null) Destroy(old.gameObject);

        coneChild = new GameObject("VisionConeMesh");
        coneChild.transform.SetParent(transform);
        coneChild.transform.localPosition = Vector3.zero;
        coneChild.transform.localRotation = Quaternion.identity;

        meshFilter = coneChild.AddComponent<MeshFilter>();
        MeshRenderer mr = coneChild.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.05f, 0.05f, 0.35f);
        mr.material             = mat;
        mr.sortingLayerName     = "Default";
        mr.sortingOrder         = 2;

        visionMesh      = new Mesh();
        visionMesh.name = "VisionConeMesh";
        meshFilter.mesh = visionMesh;

        Debug.Log("[VisionCone] Child cone object created.");
    }

    void RotateCone()
    {
        float rotateAmount = rotationSpeed * Time.deltaTime;
        currentAngle += rotateClockwise ? -rotateAmount : rotateAmount;

        while (currentAngle <    0f) currentAngle += 360f;
        while (currentAngle >= 360f) currentAngle -= 360f;
    }

    void UpdateFacingDirection()
    {
        if      (currentAngle >= 315f || currentAngle <  45f) currentFacingDirection = 4; // Right
        else if (currentAngle >=  45f && currentAngle < 135f) currentFacingDirection = 1; // Up
        else if (currentAngle >= 135f && currentAngle < 225f) currentFacingDirection = 3; // Left
        else                                                   currentFacingDirection = 2; // Down
    }

    void UpdateAnimationState()
    {
        if (animator == null) return;
        animator.SetInteger("State", currentFacingDirection * 10);
        animator.SetBool("IsAlert", isAlert);
    }

    void DrawVisionCone()
    {
        float halfVision = visionAngle * 0.5f;
        float angleStep  = visionAngle / rayCount;

        Vector3[] vertices  = new Vector3[rayCount + 2];
        int[]     triangles = new int[rayCount * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++)
        {
            float        angle = currentAngle - halfVision + angleStep * i;
            Vector2      dir   = AngleToDirection(angle);
            RaycastHit2D hit   = Physics2D.Raycast(transform.position, dir, visionRange, obstacleLayer);

            vertices[i + 1] = hit.collider != null
                ? transform.InverseTransformPoint(hit.point)
                : (Vector3)(dir * visionRange);
        }

        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices  = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    void CheckPlayerDetection()
    {
        bool playerDetectedThisFrame = false;
        float halfVision = visionAngle * 0.5f;
        float angleStep  = visionAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float        angle     = currentAngle - halfVision + angleStep * i;
            Vector2      dir       = AngleToDirection(angle);
            RaycastHit2D wallHit   = Physics2D.Raycast(transform.position, dir, visionRange, obstacleLayer);
            float        wallDist  = wallHit.collider != null ? wallHit.distance : visionRange;
            RaycastHit2D playerHit = Physics2D.Raycast(transform.position, dir, visionRange, playerLayer);

            if (playerHit.collider == null)    continue;
            if (playerHit.distance > wallDist) continue;

            PlayerMiniGame3 player = playerHit.collider.GetComponent<PlayerMiniGame3>();
            if (player == null)        continue;
            if (player.IsHiding())     continue;

            playerDetectedThisFrame = true;

            if (damageTimer <= 0f)
            {
                Debug.Log("[VisionCone] ✅ Player detected! TakeDamage()");
                if (attackSound != null)
                    audioSource.PlayOneShot(attackSound);
                player.TakeDamage();
                damageTimer = damageCooldown;
            }
            break;
        }

        isAlert = playerDetectedThisFrame;

        if ( isAlert && !wasAlertLastFrame) Debug.Log("[VisionCone] 🔴 Switched to RED!");
        if (!isAlert &&  wasAlertLastFrame) Debug.Log("[VisionCone] 🔵 Switched to BLUE!");
        wasAlertLastFrame = isAlert;
    }

    Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void OnDrawGizmosSelected()
    {
        // วาด Vision Cone
        float halfVision = visionAngle * 0.5f;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)AngleToDirection(currentAngle - halfVision) * visionRange);
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)AngleToDirection(currentAngle + halfVision) * visionRange);
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // วาด Patrol Path
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i].waypoint == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].waypoint.position, 0.15f);

            int next = (i + 1) % patrolPoints.Length;
            if (patrolPoints[next].waypoint != null)
                Gizmos.DrawLine(patrolPoints[i].waypoint.position, patrolPoints[next].waypoint.position);
        }
    }
}