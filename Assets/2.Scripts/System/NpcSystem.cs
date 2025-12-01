using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NpcSystem : MonoBehaviour
{
    [SerializeField] GameObject[] npcPrefabs = new GameObject[6];
    [SerializeField] Transform entrance;
    [SerializeField] RoadGraphFromGrid roadGraphFromGrid;

    void Start()
    {
        CityManager.Instance.dateSystem.OnDayChanged += OnDayChanged;
        StartCoroutine(SpawnFirstNPC());
    }

    void SpawnNpc()
    {
        int i = Random.Range(0, npcPrefabs.Length);
        Instantiate(npcPrefabs[i], entrance.position, Quaternion.identity);
    }

    void OnDayChanged()
    {
        SpawnNpc();
    }

    private IEnumerator SpawnFirstNPC()
    {
        yield return new WaitUntil(() => roadGraphFromGrid.AllWayPoints().Any());

        SpawnNpc();
    }
}
