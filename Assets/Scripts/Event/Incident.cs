using System;
using UnityEngine;

public enum IncidentState { Progressing, Resolving, Resolved }
public sealed class Incident
{
    public EventType EventType;
    public float CreatedAt;
    public float Deadline;        // 만료/실패 시각
    public IncidentState State;
    public float ResolvingProgress;        // Resolving 진행도(0~1)

    public Action OnSpawned;
    public Action OnResolved;
}
