using UnityEngine;

public enum EventType
{
    SecurityEvent,
    FireRiskEvent
}

public abstract class BaseEvent
{
    public float Timer;
    public EventType EventType;

    public BaseEvent(EventType eventType, float timer)
    {
        EventType = eventType;
        Timer = timer;
    }

    public abstract void Execute();
}
