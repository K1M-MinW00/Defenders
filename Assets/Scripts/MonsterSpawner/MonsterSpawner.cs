using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    public MonsterPool monsterPool;
    public WaveData waveData;
    public Transform spawnPoint;

    private List<Monster> monsterLists;
    public List<Monster> MonsterLists => monsterLists;

    public float spawnInterval = 0.1f;

    private void Awake()
    {
        monsterLists = new List<Monster>();
    }
    private void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        foreach (var subWave in waveData.subWaves)
        {
            yield return StartCoroutine(SpawnSubWave(subWave));
            yield return new WaitForSeconds(waveData.delayBetweenSubWaves);
        }

        Debug.Log("Wave finished");
    }

    private IEnumerator SpawnSubWave(SubWaveData subWave)
    {
        for (int i = 0; i < subWave.count; i++)
        {
            GameObject obj = monsterPool.Spawn(subWave.monsterId, spawnPoint.position);
            Monster monster = obj.GetComponent<Monster>();
            monsterLists.Add(monster);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
