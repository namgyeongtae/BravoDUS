using UnityEngine;

public class SecurityEventController : EventController
{
    private readonly float _baseRatePerMin;

    public SecurityEventController(float baseRatePerMin) : base(EventType.SecurityEvent)
    {
        _baseRatePerMin = baseRatePerMin;
    }

    protected override float ScheduleNext(float now, CityStat stats)
    {
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.FireRate));
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        var incident = new Incident() {
            EventType = EventType.SecurityEvent,
            CreatedAt = now,
            Deadline = now + 120f,
            ResolvingProgress = 0f,
            OnResolved = () => {
                // TODO
                // 치안 이벤트 해결 시 도시 디버프 해제
                Debug.Log("치안 이벤트 해결");
            },
            OnSpawned = () => {
                // TODO
                // 치안 이벤트 알람 UI 띄우기
                    
                // 도시에 디버프 부여
                // 적용할 디버프 기획 파악 필요
                
                Debug.Log("치안안 이벤트 스폰");
            }
        };

        Managers.Event.AddIncident(incident);

        return incident;
    }
}
