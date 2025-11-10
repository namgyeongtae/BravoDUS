using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Building Prefab DB")]
public class BuildingPrefabDB : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public BuildingType type;
        public GameObject prefab;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    public GameObject GetPrefab(BuildingType type)
    {
        foreach (var e in _entries)
        {
            if (e.type == type)
                return e.prefab;
        }

        Debug.LogError($"[BuildingPrefabDB] Prefab not found for BuildingType: {type}");
        return null;
    }
}
