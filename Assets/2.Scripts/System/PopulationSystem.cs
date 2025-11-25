using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PopulationSystem : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform entrance; // 출입구(시작 위치)

    GameObject npc;

    int currentPopulation = 1;
    int maxPopulation = 1;

    List<GameObject> npcs = new List<GameObject>();

    void Update()
    {
        Mathf.Clamp(currentPopulation, 0, CityManager.Instance.happinessSystem.GetHappiness());

        //spawnNpc();
        //RemoveLastNpc();
    }

    void spawnNpc()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            npc = Instantiate(npcPrefab, entrance.position, Quaternion.identity);
            npcs.Add(npc);
        }
    }

    void RemoveLastNpc()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (npcs.Count > 0)
            {
                GameObject target = npcs[npcs.Count - 1];   // 마지막 NPC 가져오기
                npcs.RemoveAt(npcs.Count - 1);              // 리스트에서 제거
                Destroy(target);                            // 게임 오브젝트 삭제
            }                 
        }
    }

    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }

    public int GetMaxPopulation()
    {
        return maxPopulation;
    }

    public void ApplyPopulationChange(int delta)
    {
        currentPopulation += delta;
        maxPopulation += delta;
    }

    // 인구 수 증가
    //public void AddPopulation()
    //{
    //    population++;
    //}

    // 인구 수 감소
    //public void RemovePopulation()
    //{
    //    population--;      
    //    population = Mathf.Max(population, 0); // 범위 제한
    //}
}
