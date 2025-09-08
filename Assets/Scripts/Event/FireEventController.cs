using UnityEngine;

public class FireEventController : EventController
{
    private readonly float _baseRatePerMin;

    public FireEventController(float baseRatePerMin) : base(EventType.FireRiskEvent)
    {
        _baseRatePerMin = baseRatePerMin;
    }

    protected override float ScheduleNext(float now, CityStat stats)
    {
        // λ = base * FireRate(0~1). 너무 낮으면 아주 드물게라도 나오도록 epsilon
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.FireRate));
        // 지수분포: Δt(분) = -ln(1-u)/λ
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stats)
    {
        // 화재 이벤트는 고정된 interval로 스케줄링되므로, 랜덤으로 화재 이벤트를 발생시킴
        
        var incident = new Incident() {
            EventType = EventType.FireRiskEvent,
            CreatedAt = now,
            Deadline = now + 120f,
            ResolvingProgress = 0f,
            OnResolved = () => {
                // TODO
                // 화재 이벤트 해결 시 건물 수복 (기능 정상화)
                Debug.Log("화재 이벤트 해결");
            },
            OnSpawned = () => {
                // TODO
                // 화재 이벤트 알람 UI 띄우기
                    
                // 화재 대상 건물에 디버프 부여
                // 적용할 디버프 기획 파악 필요
                // var building = Managers.Building.GetRandomBuilding();
                // building.GetDebuff();
                Debug.Log("화재 이벤트 스폰");
            }
        };

        Managers.Event.AddIncident(incident);

        return incident;
    }
}
