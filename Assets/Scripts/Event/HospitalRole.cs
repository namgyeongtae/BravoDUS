using UnityEngine;

public class HospitalRole : RoleHandler
{
    [SerializeField] private float baseHealRate = 5f; // 기본 건강 관리율

    public override void HandleEvent(string eventType)
    {
        if (eventType == "Disease")
        {
            
            Debug.Log("질병 치료: " + (baseHealRate * buildingLevel));
        }
    }

    public override void OnUpgrade(int newLevel)
    {
        base.OnUpgrade(newLevel);
        // 업그레이드 특수 로직 (예시 추가: healRate 레벨 기반 증가, 나중 커스터마이즈)
        baseHealRate *= 1.1f; // 레벨당 10% 증가 (테스트 용, 실제 컨셉 맞춰 조정)
    }
}