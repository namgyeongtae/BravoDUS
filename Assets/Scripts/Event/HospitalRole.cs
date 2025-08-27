using UnityEngine;

public class HospitalRole : RoleHandler
{
    [SerializeField] private float baseHealRate = 5f; // 기본 건강 관리율

    public override void HandleEvent(string eventType)
    {
        if (eventType == "Disease")
        {
            // 수정: Managers.Game.HealPopulation 호출 (통합)
            Managers.Game.HealPopulation(baseHealRate * buildingLevel);
            Debug.Log("질병 치료: " + (baseHealRate * buildingLevel));
        }
    }

    public override void OnUpgrade(int newLevel)
    {
        base.OnUpgrade(newLevel);
        // 업그레이드 특수 로직
    }
}