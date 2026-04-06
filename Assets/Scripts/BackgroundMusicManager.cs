using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;

    [Header("Settings")]
    [SerializeField] private float defaultFadeDuration = 1.5f;
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;

    [Header("BGM ตาม Scene (เรียงตาม Build Index)")]
    [SerializeField] private SceneMusicEntry[] sceneMusicList;

    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;   // ชื่อ Scene ให้ตรงกับใน Build Settings
        public AudioClip musicClip;
    }

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        activeSource = musicSource1;
        inactiveSource = musicSource2;

        // ฟัง event เมื่อ Scene โหลดเสร็จ
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // เรียกอัตโนมัติทุกครั้งที่เปลี่ยน Scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clipForScene = GetClipForScene(scene.name);

        if (clipForScene == null)
        {
            // ไม่มีเพลงกำหนดไว้ → หยุดเพลงเก่า
            StopMusic();
            return;
        }

        // ถ้าเพลงเดิมกำลังเล่นอยู่และเป็นเพลงเดียวกัน → ไม่ต้องทำอะไร
        if (activeSource.isPlaying && activeSource.clip == clipForScene)
            return;

        CrossfadeMusic(clipForScene);
    }

    private AudioClip GetClipForScene(string sceneName)
    {
        foreach (var entry in sceneMusicList)
        {
            if (entry.sceneName == sceneName)
                return entry.musicClip;
        }
        return null;
    }

    /// <summary>เล่นเพลงทันที (ไม่ fade)</summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        activeSource.clip = clip;
        activeSource.loop = loop;
        activeSource.volume = masterVolume;
        activeSource.Play();
    }

    /// <summary>เปลี่ยนเพลงพร้อม Crossfade</summary>
    public void CrossfadeMusic(AudioClip newClip, float fadeDuration = -1f, bool loop = true)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeRoutine(newClip, fadeDuration, loop));
    }

    /// <summary>Fade Out แล้วหยุด</summary>
    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine(activeSource, fadeDuration));
    }

    /// <summary>หยุดทันที</summary>
    public void StopMusicImmediate()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        activeSource.Stop();
        inactiveSource.Stop();
    }

    /// <summary>ปรับ volume หลัก</summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (activeSource.isPlaying)
            activeSource.volume = masterVolume;
    }

    // ─── Coroutines ──────────────────────────────────────────

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float duration, bool loop)
    {
        inactiveSource.clip = newClip;
        inactiveSource.loop = loop;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float timer = 0f;
        float startVolume = activeSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            activeSource.volume   = Mathf.Lerp(startVolume, 0f, t);
            inactiveSource.volume = Mathf.Lerp(0f, masterVolume, t);

            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;

        (activeSource, inactiveSource) = (inactiveSource, activeSource);
        activeSource.volume = masterVolume;
    }

    private IEnumerator FadeOutRoutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = 0f;
    }
}