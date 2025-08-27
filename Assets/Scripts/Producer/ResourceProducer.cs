using UnityEngine;

public class ResourceProducer : MonoBehaviour
{
    [SerializeField] private string resourceType = "Wood"; // "Wood", "Iron" 등
    [SerializeField] private float baseProductionRate = 10f; // 기본 생산량
    [SerializeField] private float productionInterval = 60f; // 주기 (초)

    private float currentRate;
    private int buildingLevel = 1;

    void Start()
    {
        currentRate = baseProductionRate;
        InvokeRepeating("Produce", productionInterval, productionInterval); // 타이머
    }

    private void Produce()
    {
        // 수정: Managers.Game.AddResource 호출 (통합)
        Managers.Game.AddResource(resourceType, currentRate);
        Debug.Log($"{resourceType} 생산: {currentRate}");
    }

    public void OnUpgrade(int newLevel)
    {
        buildingLevel = newLevel;
        currentRate = baseProductionRate * (1 + (newLevel - 1) * 0.2f); // 레벨당 20% 업
        CancelInvoke("Produce");
        InvokeRepeating("Produce", productionInterval * (1 - (newLevel - 1) * 0.1f), productionInterval * (1 - (newLevel - 1) * 0.1f));
    }
}