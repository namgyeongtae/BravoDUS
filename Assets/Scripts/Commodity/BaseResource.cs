using UnityEngine;

public abstract class BaseResource
{
    protected string _resourceName;
    protected float _amount;

    public float Amount => _amount;

    public void Gather(float addValue)
    {
        _amount += addValue;
        OnAmountChanged(_amount, isAdd: true);
    }
    public void Consume(float subValue)
    {
        _amount = Mathf.Max(0, _amount - subValue);
        OnAmountChanged(_amount, isAdd: false);
    }

    protected virtual void OnAmountChanged(float amount, bool isAdd) { }
}