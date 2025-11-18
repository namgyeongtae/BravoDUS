using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireEventController : EventController
{
    private List<FireStationRole> _fireStationRoles = new List<FireStationRole>();
    private Dictionary<Building, Incident> _incidentBuildings = new Dictionary<Building, Incident>();
    private Dictionary<Incident, UIFireEventWarning> _incidentUIWarnings = new Dictionary<Incident, UIFireEventWarning>();
    private Dictionary<Incident, WorkForce> _resolvingWorkForces = new Dictionary<Incident, WorkForce>();

    public List<FireStationRole> FireStationRoles => _fireStationRoles;
    public Dictionary<Building, Incident> IncidentBuildings => _incidentBuildings;
    public Dictionary<Incident, UIFireEventWarning> IncidentUIWarnings => _incidentUIWarnings;
    public Dictionary<Incident, WorkForce> ResolvingWorkForces => _resolvingWorkForces;
    
    public FireEventController(float baseRatePerMin) : base(EventType.FireRiskEvent)
    {
        _baseRatePerMin = baseRatePerMin;
    }

    private Building _targetBuilding = null;

    protected override float ScheduleNext(float now, CityStat stats)
    {
        /* // λ = base * FireRate(0~1). 너무 낮으면 아주 드물게라도 나오도록
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.FireRate));
        // 지수분포: Δt(분) = -ln(1-u)/λ
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f; */

        return now + 40f;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        // TODO: 조건 개선
        // 현재 완공이 되었고 디버프가 없는 건물 리스트
        // 조건이 너무 많음.... 추가되었을 때 계속 && 붙여야 함....
        // 이 구조는 상당히 좋지 않음.... 개선되야함
        var buildings = CraftingManager.Instance.Buildings.Where(b => 
                                                b.GetComponent<Building>().CurrentState != Building.State.Fired
                                             && b.GetComponent<Building>().BuildingType != BuildingType.FireStation
                                             && b.GetComponent<Building>().BuildingType != BuildingType.Hospital
                                             && b.GetComponent<Building>().BuildingType != BuildingType.PoliceStation
                                             && b.GetComponent<Building>().BuildingType != BuildingType.Government
                                             && b.CurrentState != Building.State.Ruin 
                                             && b.CurrentState != Building.State.Constructing);

        if (buildings.Count() == 0)
        {
            Debug.LogError("FireEventController: No fireable buildings");
            return null;
        }

        // 조건에 맞는 건물 중 랜덤 선정
        _targetBuilding = buildings.ElementAt(Random.Range(0, buildings.Count()));

        // 화재 차단 가능한 소방서 찾기
        for (int i = 0; i < _fireStationRoles.Count; i++)
        {
            if (_fireStationRoles[i].CanProtect(_targetBuilding))
            {
                if (SuppressedEvent(_fireStationRoles[i], _targetBuilding))
                {
                    Debug.Log("화재 차단 성공: " + _targetBuilding.name);
                    return null;
                }
            }
        }

        _targetBuilding.SetCurrentState(Building.State.Fired);

        return base.ExecuteSpawn(now, stat);
    }

    protected override void OnSpawned_Event(Incident inc)
    {
        if (_targetBuilding == null)
        {
            Debug.LogError("Target Building is NULL!");
            return;
        }

        _targetBuilding.GetComponent<RoleHandler>()?.OnDeBuff();

        _incidentBuildings.Add(_targetBuilding, inc);

        var fireVFX = Managers.Resource.Instantiate("VFX/FireSmokeVFX", _targetBuilding.transform).GetComponent<ParticleSystem>();
        fireVFX.transform.localPosition = Vector3.zero;
        fireVFX.Play();

        var uiWarning = Managers.UI.AddPanel<UIFireEventWarning>(_targetBuilding, true);
        _incidentUIWarnings.Add(inc, uiWarning);

        _targetBuilding = null;
    }

    protected override void OnResolved_Event(Incident inc)
    {
        var building = _incidentBuildings.FirstOrDefault(x => x.Value == inc).Key;

        building.GetComponent<RoleHandler>()?.OnResolved();

        var fireVFX = building.GetComponentInChildren<ParticleSystem>();
        if (fireVFX == null)
        {
            Debug.LogError("FireEventController: Fire VFX not found");
            return;
        }
        Managers.Resource.Destroy(fireVFX.gameObject);
    }

    protected override void OnUpdateTick_Event(Incident inc)
    {
        // _incidentBuildings[inc].GetComponent<RoleHandler>()?.OnDeBuff();
    }

    private bool SuppressedEvent(FireStationRole fireStationRole, Building building)
    {
        float suppressionRate = fireStationRole.SuppressionRate;
        float randomValue = Random.value;
        
        Debug.Log("Random Value: " + randomValue + " Suppression Rate: " + suppressionRate);
        Debug.Log("Suppressed: " + (randomValue <= suppressionRate));

        return randomValue <= suppressionRate;
    }

    public void AddFireStationRole(FireStationRole role)
    {
        _fireStationRoles.Add(role);
    }

    public bool IsSuppressing(WorkForce workForce)
    {
        return _resolvingWorkForces.ContainsValue(workForce);
    }
}
