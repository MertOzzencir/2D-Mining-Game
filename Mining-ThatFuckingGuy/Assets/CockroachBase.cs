using System.Collections.Generic;
using UnityEngine;

public class CockroachBase : MonoBehaviour
{
    [SerializeField] private CockroachEnemy cockroachPrefab;
    List<CockroachEnemy> spawnedCockroaches = new List<CockroachEnemy>();


    public void SpawnCockroach()
    {
        CockroachEnemy spawned = Instantiate(cockroachPrefab,transform.position,Quaternion.identity);
        spawned.OnSpawned(this);
        spawnedCockroaches.Add(spawned);
    }
}
