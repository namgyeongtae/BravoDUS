using System;
using UnityEngine;

public class UIBind : UIBindBase
{
    public Action OnClose_Event { get; set; } = null;

    protected virtual void Start()
    {
        Open();
    }

    public virtual void Open()
    {

    }

    public virtual void Close()
    {


        OnClose_Event?.Invoke();
        OnClose_Event = null;
    }
}
