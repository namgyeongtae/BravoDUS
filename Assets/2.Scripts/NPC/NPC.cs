using Unity.VisualScripting;
using UnityEngine;

public class NPC : MonoBehaviour
{
    PopulationSystem PopulationSystem;

    private void Awake()
    {
        PopulationSystem = FindFirstObjectByType<PopulationSystem>();
    }

    //private void OnEnable()
    //{
    //    PopulationSystem.AddPopulation();
    //}

    //private void OnDisable()
    //{
    //    PopulationSystem.RemovePopulation();
    //}
}
