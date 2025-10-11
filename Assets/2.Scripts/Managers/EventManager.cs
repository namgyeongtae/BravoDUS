using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : IManagerBase
{
    private Dictionary<EventType, EventController> _events = new Dictionary<EventType, EventController>();
    private List<Incident> _incidents = new List<Incident>();

    private CityStat _cityStat = new CityStat();    // Temp Code -> 생성자로 안 할 거임

    public EventController Fire => _events[EventType.FireRiskEvent] as FireEventController;
    public EventController Security => _events[EventType.SecurityEvent] as SecurityEventController;
    public EventController Injure => _events[EventType.InjureEvent] as InjureEventController;
    public void Init()
    {
        // Load Events from Database

        // _cityStat = Managers.Data.LoadData<CityStat>("CityStat"); 어쨌든 현재 유저의 도시 스탯을 로드 (이 방식이 아닐 수 있음)
    
        _events.Add(EventType.FireRiskEvent, new FireEventController(0.01f));
        // _events.Add(EventType.SecurityEvent, new SecurityEventController(0.01f));
        _events.Add(EventType.InjureEvent, new InjureEventController(0.01f));
    }

    public void Update()
    {
        float now = Time.time;

        // 1) 타입별 발생 스케줄링 & 스폰
        foreach (var ctrl in _events.Values)
        {
            ctrl.TickSchedule(now, _cityStat);
        }

        // 2) 활성 인시던트 진행도 갱신(FSM)
        for (int i = _incidents.Count - 1; i >= 0; --i)
        {
            var inc = _incidents[i];
            TickIncident(inc, Time.deltaTime, now);
        }
    }

    private void TickIncident(Incident inc, float dt, float now)
    {
        switch (inc.State)
        {
            case IncidentState.Progressing:
                {
                    inc.Tick();
                }
                break;
            case IncidentState.Resolving:
                {
                    bool canResolve = inc.EventType switch
                    {
                        EventType.SecurityEvent => _cityStat.ResponsePower > 0,
                        EventType.FireRiskEvent => _cityStat.SuppressPower > 0,
                        EventType.InjureEvent => Managers.HR.HoldResources.Where(x => x.JobType == JobType.Doctor && x.HRState == HRState.Work).Count() > 0,
                        _ => false
                    };

                    if (!canResolve)
                        return;

                    float power = inc.EventType switch
                    {
                        EventType.SecurityEvent => _cityStat.ResponsePower,
                        EventType.FireRiskEvent => _cityStat.SuppressPower,
                        EventType.InjureEvent => _cityStat.HealPower,
                        _ => throw new System.Exception($"Unknown event type: {inc.EventType}")
                    };

                    float resolveSeconds = Mathf.Max(5f, 20f / Mathf.Max(1f, power)); // 최소 5초
                    inc.ResolvingProgress += dt / resolveSeconds;
                    inc.RemainTime = inc.ResolvingProgress * resolveSeconds;

                    if (inc.ResolvingProgress >= 1f) 
                        inc.State = IncidentState.Resolved;
                }
                break;
            case IncidentState.Resolved:
                {
                    // OnResolved는 EventController에서 생성
                    RemoveIncident(inc);
                }
                break;
        }
    }

    public void AddEvent(EventType eventType, EventController evt)
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

    public void AddIncident(Incident incident)
    {
        incident.OnSpawned?.Invoke(incident);
        _incidents.Add(incident);
    }

    public void RemoveIncident(Incident incident)
    {
        incident.OnResolved?.Invoke(incident);
        _incidents.Remove(incident);
    }
    public void Release() // �߰�: ��ü ����
    {
        _events.Clear();
    }
}