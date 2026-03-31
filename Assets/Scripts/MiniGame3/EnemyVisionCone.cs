using UnityEngine;

/// <summary>
/// Enemy ที่มี Vision Cone หมุน 360 องศา + เปลี่ยน Sprite ทันทีเมื่อเจอ/ไม่เจอผู้เล่น
/// </summary>
public class EnemyVisionCone : MonoBehaviour
{
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
    public AudioClip attackSound; // ลาก AudioClip ใส่ใน Inspector
    private AudioSource audioSource;

    // Components
    private Animator    animator;
    private GameObject  coneChild;
    private MeshFilter  meshFilter;
    private Mesh        visionMesh;

    // State
    private float currentAngle = 0f;
    private int   currentFacingDirection = 2;
    private bool  isAlert = false;

    // ─────────────────────────────────────────
    void Start()
    {
        animator = GetComponent<Animator>();

        // เพิ่มบรรทัดนี้
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log($"[VisionCone] playerLayer={playerLayer.value} obstacleLayer={obstacleLayer.value}");
        if (playerLayer.value == 0)
            Debug.LogError("[VisionCone] *** playerLayer ว่างอยู่! ตั้งค่าใน Inspector! ***");

        CreateConeChildObject();
        UpdateAnimationState();
    }

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
        mr.material   = mat;
        mr.sortingLayerName = "Default";
        mr.sortingOrder     = 2;

        visionMesh      = new Mesh();
        visionMesh.name = "VisionConeMesh";
        meshFilter.mesh = visionMesh;

        Debug.Log("[VisionCone] Child cone object created.");
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (visionMesh == null) return;

        RotateCone();
        DrawVisionCone();
        UpdateFacingDirection();

        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        CheckPlayerDetection(); // ตรวจจับและอัปเดต isAlert
        UpdateAnimationState(); // อัปเดต Animator ทุก frame
    }

    // ─────────────────────────────────────────
    void RotateCone()
    {
        float rotateAmount = rotationSpeed * Time.deltaTime;
        
        if (rotateClockwise)
            currentAngle -= rotateAmount;
        else
            currentAngle += rotateAmount;

        while (currentAngle < 0f)    currentAngle += 360f;
        while (currentAngle >= 360f) currentAngle -= 360f;
    }

    // ─────────────────────────────────────────
    void UpdateFacingDirection()
    {
        if (currentAngle >= 315f || currentAngle < 45f)
            currentFacingDirection = 4; // Right
        else if (currentAngle >= 45f && currentAngle < 135f)
            currentFacingDirection = 1; // Up
        else if (currentAngle >= 135f && currentAngle < 225f)
            currentFacingDirection = 3; // Left
        else
            currentFacingDirection = 2; // Down
    }

    // ─────────────────────────────────────────
    void UpdateAnimationState()
    {
        if (animator == null) return;

        // ส่งทิศทาง (10/20/30/40)
        int idleState = currentFacingDirection * 10;
        animator.SetInteger("State", idleState);

        // ส่งสถานะ Alert (true = แดง, false = ฟ้า)
        animator.SetBool("IsAlert", isAlert);
    }

    // ─────────────────────────────────────────
    void DrawVisionCone()
    {
        float halfVision = visionAngle * 0.5f;
        float angleStep  = visionAngle / rayCount;

        Vector3[] vertices  = new Vector3[rayCount + 2];
        int[]     triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++)
        {
            float   angle = currentAngle - halfVision + angleStep * i;
            Vector2 dir   = AngleToDirection(angle);

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, dir, visionRange, obstacleLayer);

            Vector3 endpoint = hit.collider != null
                ? transform.InverseTransformPoint(hit.point)
                : (Vector3)(dir * visionRange);

            vertices[i + 1] = endpoint;
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

    // ─────────────────────────────────────────
    void CheckPlayerDetection()
    {
        // รีเซ็ต isAlert ทุก frame
        // ถ้า frame นี้ไม่เจอ Player = กลับเป็นสีฟ้าทันที
        bool playerDetectedThisFrame = false;

        float halfVision = visionAngle * 0.5f;
        float angleStep  = visionAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float   angle = currentAngle - halfVision + angleStep * i;
            Vector2 dir   = AngleToDirection(angle);

            RaycastHit2D wallHit  = Physics2D.Raycast(
                transform.position, dir, visionRange, obstacleLayer);
            float wallDist = wallHit.collider != null ? wallHit.distance : visionRange;

            RaycastHit2D playerHit = Physics2D.Raycast(
                transform.position, dir, visionRange, playerLayer);

            if (playerHit.collider == null)       continue;
            if (playerHit.distance > wallDist)    continue;

            PlayerMiniGame3 player = playerHit.collider.GetComponent<PlayerMiniGame3>();
            if (player == null) continue;
            if (player.IsHiding()) continue;

            // เจอผู้เล่น!
            playerDetectedThisFrame = true;

            // ทำ Damage (ตามระยะ cooldown)
            if (damageTimer <= 0f)
            {
                Debug.Log("[VisionCone] ✅ Player detected! TakeDamage()");

                // เล่นเสียงโจมตี
                if (attackSound != null)
                    audioSource.PlayOneShot(attackSound);

                player.TakeDamage();
                damageTimer = damageCooldown;
            }
            break; // เจอแล้วไม่ต้องเช็ค ray ต่อ
        }

        // อัปเดตสถานะ Alert ทันที
        isAlert = playerDetectedThisFrame;

        // Log เฉพาะตอนเปลี่ยนสถานะ
        if (isAlert && !wasAlertLastFrame)
            Debug.Log("[VisionCone] 🔴 Switched to RED!");
        else if (!isAlert && wasAlertLastFrame)
            Debug.Log("[VisionCone] 🔵 Switched to BLUE!");

        wasAlertLastFrame = isAlert;
    }

    private bool wasAlertLastFrame = false; // สำหรับ log

    // ─────────────────────────────────────────
    Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        float halfVision = visionAngle * 0.5f;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        
        Vector2 dir1 = AngleToDirection(currentAngle - halfVision);
        Vector2 dir2 = AngleToDirection(currentAngle + halfVision);
        
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir1 * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir2 * visionRange);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}