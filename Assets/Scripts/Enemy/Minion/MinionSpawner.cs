using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public static MinionSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] GameObject[] minionPrefabs;

    [Header("Auto Formation")]
    [SerializeField] int rows = 2;
    [SerializeField] int columns = 4;
    [SerializeField] float spacingX = 2f;
    [SerializeField] float spacingY = 1.5f;
    [SerializeField] Vector2 formationCenter = new Vector2(0f, 6f);

    [Header("Spawn")]
    [SerializeField] float spawnDelay = 0.3f;
    [SerializeField] Vector2 spawnLeft = new Vector2(-10f, 6f);
    [SerializeField] Vector2 spawnRight = new Vector2(10f, 6f);

    [Header("Wave")]
    [SerializeField] bool autoWave = true;
    [SerializeField] float waveDelay = 5f;

    [Header("Sound Effects")]
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] float soundVolume = 0.8f;

    List<GameObject> minions = new List<GameObject>();
    int currentWave = 0;
    AudioSource audioSource;
    float cleanupTimer;

    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveEnd;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
    }

    void Start()
    {
        if (autoWave) StartCoroutine(AutoWaveLoop());
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnValidate()
    {
        foreach (var prefab in minionPrefabs)
        {
            if (prefab != null && prefab.GetComponent<MinionController>() == null)
            {
                Debug.LogWarning($"[MinionSpawner] Prefab '{prefab.name}' tidak memiliki MinionController!");
            }
        }
    }

    IEnumerator AutoWaveLoop()
    {
        while (autoWave)
        {
            if (minions.Count == 0)
            {
                currentWave++;
                yield return SpawnWave();
                yield return new WaitForSeconds(waveDelay);
            }
            yield return null;
        }
    }

    public void StopAutoWave()
    {
        autoWave = false;
        StopCoroutine(AutoWaveLoop());
    }

    public IEnumerator SpawnWave()
    {
        OnWaveStart?.Invoke(currentWave);

        Vector3[] positions = GenerateFormation();
        foreach (Vector3 pos in positions)
        {
            SpawnMinion(pos);
            yield return new WaitForSeconds(spawnDelay);
        }

        OnWaveEnd?.Invoke(currentWave);
    }

    Vector3[] GenerateFormation()
    {
        List<Vector3> positions = new List<Vector3>();

        float totalWidth = (columns - 1) * spacingX;
        float totalHeight = (rows - 1) * spacingY;

        Vector2 startPos = formationCenter - new Vector2(totalWidth / 2f, totalHeight / 2f);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = startPos + new Vector2(col * spacingX, row * spacingY);
                positions.Add(pos);
            }
        }

        return positions.ToArray();
    }

    void SpawnMinion(Vector3 targetPos)
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0)
        {
            Debug.LogError("[MinionSpawner] No minion prefabs assigned!");
            return;
        }

        GameObject randomPrefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
        Vector2 spawnPos = Random.value > 0.5f ? spawnRight : spawnLeft;
        GameObject minion = Instantiate(randomPrefab, spawnPos, Quaternion.identity);

        MinionController controller = minion.GetComponent<MinionController>();
        controller.SetTarget(targetPos);

        minions.Add(minion);
    }

    void Update()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer >= 1f)
        {
            cleanupTimer = 0f;
            minions.RemoveAll(m => m == null);
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitSound, soundVolume);
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(deathSound, soundVolume);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3[] positions = GenerateFormation();
        foreach (Vector3 pos in positions)
        {
            Gizmos.DrawWireSphere(pos, 0.3f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnLeft, 0.5f);
        Gizmos.DrawWireSphere(spawnRight, 0.5f);
    }
}
