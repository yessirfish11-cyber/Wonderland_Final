using UnityEngine;

public class SpawnObjHits : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Range(0f, 100f)] public float weight = 1f; // โอกาส Spawn ยิ่งมาก = โอกาสออกมากขึ้น
    }

    public SpawnEntry[] objhits;

    public float maxX;
    public float minX;
    public float maxY;
    public float minY;

    public float timeBetweenSpawn;
    private float spawnTime;

    [Header("No-Spawn Zones")]
    public Transform[] noSpawnZones;
    public Vector2[] noSpawnZoneSizes;

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
            return;

        if (Time.time > spawnTime)
        {
            Spawn();
            spawnTime = Time.time + timeBetweenSpawn;
        }
    }

    void Spawn()
    {
        Vector3 spawnPos;
        int attempts = 0;

        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            spawnPos = transform.position + new Vector3(randomX, randomY, 0);
            attempts++;
        }
        while (IsInNoSpawnZone(spawnPos) && attempts < 10);

        if (IsInNoSpawnZone(spawnPos)) return;

        GameObject selectedPrefab = GetWeightedRandom();
        if (selectedPrefab != null)
            Instantiate(selectedPrefab, spawnPos, transform.rotation);
    }

    GameObject GetWeightedRandom()
    {
        float totalWeight = 0f;
        foreach (var entry in objhits)
            totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in objhits)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.prefab;
        }

        return null;
    }

    bool IsInNoSpawnZone(Vector3 pos)
    {
        for (int i = 0; i < noSpawnZones.Length; i++)
        {
            if (i >= noSpawnZoneSizes.Length) break;
            if (noSpawnZones[i] == null) continue;

            Vector3 zonePos = noSpawnZones[i].position;
            Vector2 size = noSpawnZoneSizes[i];

            bool inX = pos.x >= zonePos.x - size.x / 2 && pos.x <= zonePos.x + size.x / 2;
            bool inY = pos.y >= zonePos.y - size.y / 2 && pos.y <= zonePos.y + size.y / 2;

            if (inX && inY) return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (noSpawnZones == null || noSpawnZoneSizes == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        for (int i = 0; i < noSpawnZones.Length; i++)
        {
            if (i >= noSpawnZoneSizes.Length) break;
            if (noSpawnZones[i] == null) continue;

            Gizmos.DrawCube(noSpawnZones[i].position, new Vector3(noSpawnZoneSizes[i].x, noSpawnZoneSizes[i].y, 0.1f));
        }
    }
}