using UnityEngine;

public abstract class BaseResource
{
    protected string _resourceName;
    protected float _amount;

    public float Amount => _amount;

    public void Gather(int amount)
    {
        _amount += amount;
        OnAmountChanged();
    }
    public void Consume(int amount)
    {
        _amount -= amount;
        OnAmountChanged();
    }

    protected virtual void OnAmountChanged() { }
}