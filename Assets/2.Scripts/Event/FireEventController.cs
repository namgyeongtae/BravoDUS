using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireEventController : EventController
{
    private Dictionary<Incident, Building> _incidentBuildings = new Dictionary<Incident, Building>();

    public FireEventController(float baseRatePerMin) : base(EventType.FireRiskEvent)
    {
        _baseRatePerMin = baseRatePerMin;
    }

    protected override float ScheduleNext(float now, CityStat stats)
    {
        // λ = base * FireRate(0~1). 너무 낮으면 아주 드물게라도 나오도록
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.FireRate));
        // 지수분포: Δt(분) = -ln(1-u)/λ
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        // 현재 완공이 되었고 디버프가 없는 건물 리스트
        var buildings = CraftingManager.Instance.Buildings.Where(b => 
                                                b.GetComponent<RoleHandler>()?.DebuffCount <= 0
                                             && b.CurrentState != Building.State.Ruin 
                                             && b.CurrentState != Building.State.Constructing);

        if (buildings.Count() == 0)
        {
            Debug.LogError("FireEventController: No fireable buildings");
            return null;
        }

        return base.ExecuteSpawn(now, stat);
    }

    protected override void OnSpawned_Event(Incident inc)
    {
        // 현재 완공이 되었고 디버프가 없는 건물 리스트
        var buildings = CraftingManager.Instance.Buildings.Where(b => 
                                                b.GetComponent<RoleHandler>()?.DebuffCount <= 0
                                             && b.CurrentState != Building.State.Ruin 
                                             && b.CurrentState != Building.State.Constructing);

        if (buildings.Count() == 0)
        {
            Debug.LogError("화재 이벤트 스폰 실패: 화재 가능한 건물이 없습니다.");
            Managers.Event.RemoveIncident(inc);
            return;
        }

        // 조건에 맞는 건물 중 랜덤 선정
        var building = buildings.ElementAt(Random.Range(0, buildings.Count()));
        building.GetComponent<RoleHandler>()?.OnDeBuff();

        _incidentBuildings.Add(inc, building);

        var fireVFX = Managers.Resource.Instantiate("VFX/FireSmokeVFX", building.transform).GetComponent<ParticleSystem>();
        fireVFX.transform.localPosition = Vector3.zero;
        fireVFX.Play();

        Managers.UI.AddPanel<UIEventWarning>(building, true);
    }

    protected override void OnResolved_Event(Incident inc)
    {
        var building = _incidentBuildings[inc];

        building.GetComponent<RoleHandler>()?.OnResolved();

        var fireVFX = building.GetComponent<ParticleSystem>();
        Managers.Resource.Destroy(fireVFX.gameObject);
    }

    protected override void OnUpdateTick_Event(Incident inc)
    {
        _incidentBuildings[inc].GetComponent<RoleHandler>()?.OnDeBuff();
    }
}
