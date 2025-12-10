using UnityEngine;

public abstract class RoleHandler : MonoBehaviour
{
    protected readonly int MAX_DEBUFF_COUNT = 3;
    
    protected int buildingLevel = 1;
    protected int debuffCount = 0;
    protected bool _isActivate = false;

    public int DebuffCount => debuffCount;

    public abstract void HandleEvent(string eventType); // �̺�Ʈ ó��

    public virtual void Initialize() { }

    public virtual void OnUpgrade(int newLevel)
    {
        buildingLevel = newLevel;
    }

    public virtual void OnDeBuff() { }

    public virtual void OnResolved() { }

    // 외곽 인접 타일 중 Road 가 있으면 활성화
    public virtual void OnActivate()
    {
        if (_isActivate) return;

        var roadWarning = GetComponentInChildren<Billboard>(true);

        if (roadWarning != null)
        {
            roadWarning.gameObject.SetActive(false);
        }

        Managers.UI.GetUI<SceneUI>().BuildingHappiness(GetComponent<Building>());
        Managers.UI.GetUI<SceneUI>().BuildingPopulation(GetComponent<Building>());

        _isActivate = true;
    }

    // 외곽 인접 타일 중 Road 가 없으면 비활성화
    public virtual void OnDeActivate()
    {
        var roadWarning = GetComponentInChildren<Billboard>(true);

        if (roadWarning != null)
        {
            roadWarning.gameObject.SetActive(true);
        }
        
        if (!_isActivate) return;

        Managers.UI.GetUI<SceneUI>().DeactivateBuildingHappiness(GetComponent<Building>());
        Managers.UI.GetUI<SceneUI>().DeactivateBuildingPopulation(GetComponent<Building>());

        _isActivate = false;
    }
}
