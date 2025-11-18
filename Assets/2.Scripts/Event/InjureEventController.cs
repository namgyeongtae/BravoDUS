using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InjureEventController : EventController
{
    private Dictionary<Incident, WorkForce> _incidentWorkForces = new Dictionary<Incident, WorkForce>();

    public Dictionary<Incident, WorkForce> IncidentWorkForces => _incidentWorkForces;

    public InjureEventController(float baseRatePerMin) 
        : base(EventType.InjureEvent)
    {
        _baseRatePerMin = baseRatePerMin;
    }

    protected override Incident ExecuteSpawn(float now, CityStat stat)
    {
        if (Managers.HR.HoldResources.Where(x => x.HRState == HRState.Work).Count() <= 0)
        {
            Debug.Log("InjureEventController: No hold resources");
            return null;
        }
        
        return base.ExecuteSpawn(now, stat);
    }

    protected override void OnResolved_Event(Incident inc)
    {
        _incidentWorkForces[inc].OnHealed();
        _incidentWorkForces.Remove(inc);

        var hospital = CraftingManager.Instance.Buildings.FirstOrDefault(x => x.BuildingType == BuildingType.Hospital).GetComponent<HospitalRole>();
        hospital.RemovePatient(inc);
    }

    protected override void OnSpawned_Event(Incident inc)
    {
        var workForces = Managers.HR.HoldResources.Where(x => x.JobType != JobType.Doctor && x.HRState == HRState.Work);
        
        if (workForces.Count() <= 0)
            return;

        var workForce = workForces.ElementAt(Random.Range(0, workForces.Count()));

        _incidentWorkForces.Add(inc, workForce);

        workForce.OnInjured();
    }

    protected override void OnUpdateTick_Event(Incident inc)
    {
        return;
    }

    protected override float ScheduleNext(float now, CityStat stats)
    {
        // λ = base * FireRate(0~1). 너무 낮으면 아주 드물게라도 나오도록
        float lambda = Mathf.Max(0.08f, _baseRatePerMin * Mathf.Clamp01(1 - stats.InjureRate));
        // 지수분포: Δt(분) = -ln(1-u)/λ
        float u = Random.value;
        float minutes = -Mathf.Log(1f - u) / lambda;
        return now + minutes * 60f;
    }
}
