using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages wave spawning cycles:
/// 1. Wait between waves
/// 2. Show red blinking "!" warning on screen center
/// 3. Spawn wave that moves left to right
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject wavePrefab;
    public float minWaveSpeed = 5f;
    public float maxWaveSpeed = 8f;
    public float timeBetweenWaves = 5f;      // Seconds between each wave cycle
    public float warningDuration = 2f;        // How long "!" shows before wave appears

    [Header("Wave Spawn Position")]
    public float waveSpawnOffsetX = -15f;    // Offset to the LEFT of player (e.g. -15 = spawn 15 units left of player)
    public float waveDestroyOffsetX = 25f;   // Offset to the RIGHT of player when wave should be destroyed
    public float waveY = 0f;                  // Height of wave (usually 0)
    public float waveHeight = 10f;            // Tall enough to cover the screen vertically

    [Header("Warning UI")]
    public GameObject warningUI;              // A UI GameObject with "!" text/image
    public float blinkRate = 0.2f;           // Seconds per blink

    private bool isRunning = false;
    private Transform playerTransform;        // Cached player transform

    void Start()
    {
        if (warningUI != null)
            warningUI.SetActive(false);

        // Cache player transform
        PlayerControllerMiniGame5 player = FindFirstObjectByType<PlayerControllerMiniGame5>();
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("WaveManager: Could not find PlayerController in scene!");

        StartWaveCycle();
    }

    public void StartWaveCycle()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(WaveCycleRoutine());
        }
    }

    public void StopWaveCycle()
    {
        isRunning = false;
        StopAllCoroutines();
        if (warningUI != null)
            warningUI.SetActive(false);
    }

    IEnumerator WaveCycleRoutine()
    {
        while (isRunning)
        {
            // Wait between waves
            yield return new WaitForSeconds(timeBetweenWaves);

            // Show warning
            yield return StartCoroutine(ShowWarning());

            // Spawn the wave
            SpawnWave();
        }
    }

    IEnumerator ShowWarning()
    {
        float elapsed = 0f;
        bool visible = true;

        if (warningUI != null)
        {
            while (elapsed < warningDuration)
            {
                warningUI.SetActive(visible);
                visible = !visible;
                yield return new WaitForSeconds(blinkRate);
                elapsed += blinkRate;
            }
            warningUI.SetActive(false);
        }
        else
        {
            // Fallback: just wait
            yield return new WaitForSeconds(warningDuration);
        }
    }

    void SpawnWave()
    {
        if (wavePrefab == null)
        {
            Debug.LogWarning("WaveManager: No wavePrefab assigned!");
            return;
        }

        // Get player's X at the moment of spawning (after warning ends)
        float originX = (playerTransform != null) ? playerTransform.position.x : 0f;

        float spawnX   = originX + waveSpawnOffsetX;    // Far left of player
        float destroyX = originX + waveDestroyOffsetX;  // Far right of player

        float randomSpeed = Random.Range(minWaveSpeed, maxWaveSpeed);
        Debug.Log($"Spawning wave at X={spawnX} (player X={originX}), speed={randomSpeed}");

        GameObject waveObj = Instantiate(wavePrefab);
        Wave wave = waveObj.GetComponent<Wave>();

        if (wave != null)
        {
            wave.Init(randomSpeed, spawnX, waveY, destroyX);
        }

        // Scale wave tall enough to cover vertical play area
        waveObj.transform.localScale = new Vector3(
            waveObj.transform.localScale.x,
            waveHeight,
            1f
        );
    }
}