using System.Collections.Generic;
using UnityEngine;

public class EventManager : IManagerBase
{
    private Dictionary<EventType, BaseEvent> _events = new();

    public void Init()
    {
        // Load Events from Database
    }

    public void Update()
    {
        foreach (var evt in _events.Values)
        {
            evt.Timer -= Time.deltaTime;

            if (evt.Timer <= 0)
            {
                evt.Execute();
                RemoveEvent(evt.EventType);
            }
        }
    }

    public void AddEvent(EventType eventType, BaseEvent evt)
    {
        if (_events.ContainsKey(eventType))
        {
            Debug.LogWarning($"Event {eventType} already exists");
            return;
        }

        _events.Add(eventType, evt);
    }

    private void RemoveEvent(EventType eventType)
    {
        if (!_events.ContainsKey(eventType))
        {
            Debug.LogWarning($"Event {eventType} not found");
            return;
        }

        _events.Remove(eventType);
    }
}
