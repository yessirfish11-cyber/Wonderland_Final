using UnityEngine;

/// <summary>
/// แนบ Script นี้ไว้บน Enemy (Parent)
/// Script จะสร้าง Child Object ชื่อ "VisionConeMesh" ให้อัตโนมัติ
/// เพื่อแยก MeshRenderer ออกจาก SpriteRenderer ของ Enemy
/// </summary>
public class EnemyVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionAngle    = 180f;
    public float visionRange    = 5f;
    public float rotationSpeed  = 45f;

    [Header("Detection")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Damage")]
    public float damageCooldown = 1.0f;
    private float damageTimer   = 0f;

    [Header("Cone Mesh")]
    public int rayCount = 30;

    // Mesh วาดบน Child Object แยก
    private GameObject  coneChild;
    private MeshFilter  meshFilter;
    private Mesh        visionMesh;

    private float currentAngle = 0f;
    private float rotateDir    = 1f;

    // ─────────────────────────────────────────
    void Start()
    {
        Debug.Log($"[VisionCone] playerLayer={playerLayer.value} obstacleLayer={obstacleLayer.value}");
        if (playerLayer.value == 0)
            Debug.LogError("[VisionCone] *** playerLayer ว่างอยู่! ตั้งค่าใน Inspector! ***");

        CreateConeChildObject();
    }

    // สร้าง Child Object สำหรับ Mesh แยกจาก SpriteRenderer ของ Enemy
    void CreateConeChildObject()
    {
        // ลบ Child เก่าทิ้งก่อน (กรณี hot-reload)
        Transform old = transform.Find("VisionConeMesh");
        if (old != null) Destroy(old.gameObject);

        coneChild = new GameObject("VisionConeMesh");
        coneChild.transform.SetParent(transform);
        coneChild.transform.localPosition = Vector3.zero;
        coneChild.transform.localRotation = Quaternion.identity;

        // MeshFilter + MeshRenderer อยู่บน Child ไม่ชนกับ SpriteRenderer ของ Parent
        meshFilter = coneChild.AddComponent<MeshFilter>();
        MeshRenderer mr = coneChild.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.05f, 0.05f, 0.35f);
        mr.material   = mat;
        mr.sortingLayerName = "Default";
        mr.sortingOrder     = 2; // วาดทับ background

        visionMesh      = new Mesh();
        visionMesh.name = "VisionConeMesh";
        meshFilter.mesh = visionMesh;

        Debug.Log("[VisionCone] Child cone object created successfully.");
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (visionMesh == null) return;

        RotateCone();
        DrawVisionCone();

        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        CheckPlayerDetection();
    }

    // ─────────────────────────────────────────
    void RotateCone()
    {
        currentAngle += rotateDir * rotationSpeed * Time.deltaTime;

        if (currentAngle >= 90f)  { currentAngle = 90f;  rotateDir = -1f; }
        if (currentAngle <= -90f) { currentAngle = -90f; rotateDir =  1f; }
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

            // ใช้ InverseTransformPoint ของ Parent (Enemy) เพราะ Mesh อยู่บน Child
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
        if (damageTimer > 0f) return;

        float halfVision = visionAngle * 0.5f;
        float angleStep  = visionAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float   angle = currentAngle - halfVision + angleStep * i;
            Vector2 dir   = AngleToDirection(angle);

            // เช็คกำแพงก่อน
            RaycastHit2D wallHit  = Physics2D.Raycast(
                transform.position, dir, visionRange, obstacleLayer);
            float wallDist = wallHit.collider != null ? wallHit.distance : visionRange;

            // เช็ค Player เฉพาะ playerLayer
            RaycastHit2D playerHit = Physics2D.Raycast(
                transform.position, dir, visionRange, playerLayer);

            if (playerHit.collider == null)       continue;
            if (playerHit.distance > wallDist)    continue; // กำแพงบัง

            PlayerMiniGame3 player = playerHit.collider.GetComponent<PlayerMiniGame3>();
            if (player == null)
            {
                Debug.LogWarning($"[VisionCone] Hit '{playerHit.collider.name}' แต่ไม่มี PlayerMiniGame3!");
                continue;
            }
            if (player.IsHiding()) continue;

            Debug.Log("[VisionCone] ✅ Player detected! TakeDamage()");
            player.TakeDamage();
            damageTimer = damageCooldown;
            return;
        }
    }

    // ─────────────────────────────────────────
    // แปลงมุมเป็น Direction โดยอิง rotation ของ Enemy
    Vector2 AngleToDirection(float angleDeg)
    {
        float worldAngle = transform.eulerAngles.z + angleDeg;
        float rad        = worldAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        float halfVision = visionAngle * 0.5f;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)AngleToDirection(currentAngle - halfVision) * visionRange);
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)AngleToDirection(currentAngle + halfVision) * visionRange);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}