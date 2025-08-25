using UnityEngine;

public abstract class BaseResource
{
    protected string _resourceName;
    protected float _amount;

    public float Amount => _amount;

    public void Gather(float addValue)
    {
        _amount += addValue;
        OnAmountChanged(_amount);
    }
    public void Consume(float subValue)
    {
        _amount -= subValue;
        OnAmountChanged(_amount);
    }

    protected virtual void OnAmountChanged(float amount) { }
}