using System;
using UnityEngine;

public enum IncidentState { Progressing, Resolving, Resolved }
public sealed class Incident
{
    public EventType EventType;
    public IncidentState State;
    public float ResolvingProgress;        // Resolving 진행도(0~1)
    public float RemainTime;

    public Action<Incident> OnSpawned;
    public Action<Incident> OnResolved;
    public Action<Incident> OnUpdateTick;

    private int _tickCount = 0;

    public void Tick()
    {
        _tickCount++;
        if (_tickCount >= 10000)
        {
            _tickCount = 0;
            // 디버프 누적 (강화)
            OnUpdateTick?.Invoke(this);
        }
    }

    public void InvokeSpawnEvent()
    {
        OnSpawned?.Invoke(this);
    }

    public void InvokeResolvedEvent()
    {
        OnResolved?.Invoke(this);
    }
}
