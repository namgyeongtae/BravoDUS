using System.Collections.Generic;
using UnityEngine;

public class HospitalRole : RoleHandler
{
    [SerializeField] private float baseHealRate = 5f;

    private int _maxPatients = 3;
    private int _currentPatients => _injurePatients.Count;

    private List<Incident> _injurePatients = new List<Incident>();

    public override void HandleEvent(string eventType)
    {
        foreach (var inc in _injurePatients)
        {
            if (inc.State == IncidentState.Resolved)
            {
                RemovePatient(inc);
            }
        }
    }

    public override void OnUpgrade(int newLevel)
    {
        base.OnUpgrade(newLevel);
    }

    public void AddPatient(Incident inc)
    {
        if (inc.EventType != EventType.InjureEvent)
        {
            Debug.LogError("InjureEvent가 아닌 이벤트에 환자가 추가되었습니다.");
            return;
        }

        if (_currentPatients >= _maxPatients)
        {
            Managers.UI.AddPanel<UIToastPopup>().SettingPopup("병동이 가득 찼습니다.");
            return;
        }
            
        _injurePatients.Add(inc);

        inc.State = IncidentState.Resolving;
    }

    public void RemovePatient(Incident inc)
    {
        _injurePatients.Remove(inc);
    }
}