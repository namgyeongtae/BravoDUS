using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NpcSystem : MonoBehaviour
{
    [SerializeField] GameObject[] npcPrefabs = new GameObject[6];
    [SerializeField] Transform entrance;
    [SerializeField] RoadGraphFromGrid roadGraphFromGrid;
    [SerializeField] PopulationSystem populationSystem;

    void Start()
    {
        StartCoroutine(SpawnFirstNPC());
    }

    private void OnEnable()
    {
        populationSystem.OnPopulationChanged += OnPopulationChanged;
    }

    private void OnDisable()
    {
        populationSystem.OnPopulationChanged -= OnPopulationChanged;
    }

    void SpawnNpc()
    {
        int i = Random.Range(0, npcPrefabs.Length);
        Instantiate(npcPrefabs[i], entrance.position, Quaternion.identity);
    }

    void OnPopulationChanged()
    {
        SpawnNpc();
    }

    private IEnumerator SpawnFirstNPC()
    {
        yield return new WaitUntil(() => roadGraphFromGrid.AllWayPoints().Any());

        SpawnNpc();
    }
}
