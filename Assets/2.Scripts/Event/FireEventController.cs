using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireEventController : EventController
{
    private List<FireStationRole> _fireStationRoles = new List<FireStationRole>();
    private Dictionary<Building, Incident> _incidentBuildings = new Dictionary<Building, Incident>();
    private Dictionary<Incident, UIFireEventWarning> _incidentUIWarnings = new Dictionary<Incident, UIFireEventWarning>();
    private Dictionary<Incident, WorkForce> _resolvingWorkForces = new Dictionary<Incident, WorkForce>();

    private HashSet<BuildingType> _excludeBuildingTypes = new HashSet<BuildingType> // 화재 발생 대상에서 제외되는 Building Type
    {
        BuildingType.FireStation,
        BuildingType.Hospital,
        BuildingType.PoliceStation,
        BuildingType.Government,
    };

    private HashSet<Building.State> _excludeStates = new HashSet<Building.State> // 화재 발생 대상에서 제외되는 Building State
    {
        Building.State.Ruin,
        Building.State.Constructing,
        Building.State.Fired
    };


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
        // λ = base * FireRate(0~1). 너무 낮으면 아주 드물게라도 나오도록
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.FireRate));
        // 지수분포: Δt(분) = -ln(1-u)/λ
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        var buildings = CraftingManager.Instance.Buildings.Where(b => IsFireableBuilding(b));

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

        /* var fireVFX = Managers.Resource.InstantiateAddressable("FireSmokeVFX", Vector3.zero, Quaternion.identity, _targetBuilding.transform).GetComponent<ParticleSystem>();
        // fireVFX.transform.localPosition = Vector3.zero;
        fireVFX.Play(); */

        _targetBuilding.OnFire();

        var uiWarning = Managers.UI.AddPanel<UIFireEventWarning>(_targetBuilding, true);
        _incidentUIWarnings.Add(inc, uiWarning);

        _targetBuilding = null;
    }

    protected override void OnResolved_Event(Incident inc)
    {
        var building = _incidentBuildings.FirstOrDefault(x => x.Value == inc).Key;

        building.GetComponent<RoleHandler>()?.OnResolved();

        building.OffFire();
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

    private bool IsFireableBuilding(Building building)
    {
        if (_excludeBuildingTypes.Contains(building.BuildingType))
            return false;
        if (_excludeStates.Contains(building.CurrentState))
            return false;

        return true;
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
