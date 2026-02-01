using UnityEngine;

public class SpawnObjHits : MonoBehaviour
{
    public GameObject[] objhits;

    public float maxX;
    public float minX;
    public float maxY;
    public float minY;

    public float timeBetweenSpawn;
    private float spawnTime;

    void Update()
    {
        if (Time.time > spawnTime)
        {
            Spawn();
            spawnTime = Time.time + timeBetweenSpawn;
        }
    }

    void Spawn()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        int randomIndex = Random.Range(0, objhits.Length);
        GameObject selectedPrefab = objhits[randomIndex];

        Instantiate(
            selectedPrefab,
            transform.position + new Vector3(randomX, randomY, 0),
            transform.rotation
        );
    }
}
