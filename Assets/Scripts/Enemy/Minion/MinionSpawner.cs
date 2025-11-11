using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] GameObject minionPrefab;
    
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
    
    List<GameObject> minions = new List<GameObject>();
    int currentWave = 0;

    void Start()
    {
        if (autoWave) StartCoroutine(AutoWaveLoop());
    }

    IEnumerator AutoWaveLoop()
    {
        while (true)
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

    public IEnumerator SpawnWave()
    {
        Vector3[] positions = GenerateFormation();
        
        foreach (Vector3 pos in positions)
        {
            SpawnMinion(pos);
            yield return new WaitForSeconds(spawnDelay);
        }
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
        Vector2 spawnPos = Random.value > 0.5f ? spawnRight : spawnLeft;
        GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        
        MinionController controller = minion.GetComponent<MinionController>();
        if (controller == null) controller = minion.AddComponent<MinionController>();
        controller.SetTarget(targetPos);
        
        minions.Add(minion);
    }

    void Update()
    {
        minions.RemoveAll(m => m == null);
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