using UnityEngine;

public abstract class RoleHandler : MonoBehaviour
{
    protected readonly int MAX_DEBUFF_COUNT = 3;
    
    protected int buildingLevel = 1;
    protected int debuffCount = 0;

    public int DebuffCount => debuffCount;

    public abstract void HandleEvent(string eventType); // �̺�Ʈ ó��

    public virtual void Initialize() { }

    public virtual void OnUpgrade(int newLevel)
    {
        buildingLevel = newLevel;
    }

    public virtual void OnDeBuff() { }

    public virtual void OnResolved() { }
}
