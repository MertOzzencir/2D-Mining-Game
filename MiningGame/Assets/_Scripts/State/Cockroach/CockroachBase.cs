using System;
using System.Collections.Generic;
using UnityEngine;

public class CockroachBase : MonoBehaviour
{
    [SerializeField] private CockroachEnemy cockroachPrefab;
    [SerializeField] private CockroachSpawnData[] cockroachSpawnLevels;
    private CockroachSpawnData currentLevel;
    private List<CockroachEnemy> spawnedCockroaches = new List<CockroachEnemy>();
    private DungeonManager currentDungeon;
    private float timer;
    private int currentLevelIndex;
    void Awake()
    {
        currentLevel = cockroachSpawnLevels[0];
    }


    public void Update()
    {
        timer += Time.deltaTime;
        if (currentLevel != null && timer >= currentLevel.Cooldown)
        {
            SpawnCockroach(currentLevel.Amount);
            timer = 0;
        }
    }
    public void SpawnCockroach(int amount)
    {
        currentLevelIndex++;
        for (int i = 0; i < amount; i++)
        {
            CockroachEnemy spawned = Instantiate(cockroachPrefab, transform.position, Quaternion.identity);
            spawned.OnSpawned(this, currentDungeon);
            spawnedCockroaches.Add(spawned);
        }
        if (currentLevelIndex < cockroachSpawnLevels.Length)
        {
            currentLevel = cockroachSpawnLevels[currentLevelIndex];
        }
        else
        {
            currentLevel = null;
        }

    }
    public void OnSelfCreated(DungeonManager currentM)
    {
        currentDungeon = currentM;
    }
    public void LetChildrenPlayerHasEntered()
    {
        foreach (var a in spawnedCockroaches)
        {
            if (a.IsReturningToBase) a.StateMachine.ChangeState(a.IdleState);
        }
    }
}

[Serializable]
public class CockroachSpawnData
{
    public int Amount;
    public int Cooldown;
}
