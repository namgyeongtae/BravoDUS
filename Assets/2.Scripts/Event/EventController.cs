using System;
using UnityEngine;

public enum EventType
{
    SecurityEvent,
    FireRiskEvent,
    InjureEvent
}

public abstract class EventController
{
    protected EventType _eventType;
    protected float _nextSpawnAt;
    protected bool _initialized = false;
    protected float _baseRatePerMin;

    public string RemainTime
    {
        get
        {
            float remainingTime = _nextSpawnAt - Time.time;
            if (remainingTime <= 0) return "00:00:00";
            
            int hours = Mathf.FloorToInt(remainingTime / 3600f);
            int minutes = Mathf.FloorToInt(remainingTime % 3600f / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }

    protected EventController(EventType eventType)
    {
        _eventType = eventType;

        // TODO
        // 나중에 DB에서 가져와 _nextSpawnAt 값 설정 및 _initialized 값 설정 (이전에 초기화된 적이 있는지 확인)
        {
            // _nextSpawnAt = Managers.Data.LoadData<SavedEventData>(eventType, "NextSpawnAt");
            // _initialized = Managers.Data.LoadData<SavedEventData>(eventType, "Initialized");
        }
    }

    public void TickSchedule(float now, CityStat stats)
    {
        if (!_initialized)
        {
            _nextSpawnAt = ScheduleNext(now, stats);
            _initialized = true;
        }

        // 시간 도달 시 1건 스폰
        if (now >= _nextSpawnAt)
        {
            var inc = ExecuteSpawn(now, stats);
            if (inc != null) 
            {
                Managers.Event.AddIncident(inc);
            }

            // 다음 스케줄
            _nextSpawnAt = ScheduleNext(now, stats);
        }
    }
    protected virtual Incident ExecuteSpawn(float now, CityStat stat)
    {
        var incident = new Incident() {
            EventType = _eventType,
            ResolvingProgress = 0f,
            OnResolved = OnResolved_Event,
            OnSpawned = OnSpawned_Event,
            OnUpdateTick = OnUpdateTick_Event
        };

        incident.InvokeSpawnEvent();

        return incident;
    }

    protected abstract void OnSpawned_Event(Incident inc);
    protected abstract void OnResolved_Event(Incident inc);
    protected abstract void OnUpdateTick_Event(Incident inc);
    protected abstract float ScheduleNext(float now, CityStat stats);
}
