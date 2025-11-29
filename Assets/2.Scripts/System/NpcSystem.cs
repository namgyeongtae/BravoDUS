using UnityEngine;

public class NpcSystem : MonoBehaviour
{
    [SerializeField] GameObject npcPrefab;
    [SerializeField] Transform entrance;

    void Start()
    {
        CityManager.Instance.dateSystem.OnDayChanged += OnDayChanged;
        Invoke("SpawnNpc", 0.2f);
    }

    void SpawnNpc()
    {
        Instantiate(npcPrefab, entrance.position, Quaternion.identity);
    }

    void OnDayChanged()
    {
        SpawnNpc();
    }
}
