using UnityEngine;

public abstract class RoleHandler : MonoBehaviour
{
    protected int buildingLevel = 1;

    public abstract void HandleEvent(string eventType); // 이벤트 처리

    public virtual void OnUpgrade(int newLevel)
    {
        buildingLevel = newLevel;
    }
}
