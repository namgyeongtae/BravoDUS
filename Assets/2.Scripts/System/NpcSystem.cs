using UnityEngine;
using System.Collections.Generic;

public class NpcSystem : MonoBehaviour
{
    [SerializeField] GameObject[] npcPrefabs = new GameObject[6];
    [SerializeField] Transform entrance;

    void Start()
    {
        CityManager.Instance.dateSystem.OnDayChanged += OnDayChanged;
        Invoke("SpawnNpc", 0.2f);
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
}
