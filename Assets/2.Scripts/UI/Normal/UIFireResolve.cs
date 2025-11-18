using UnityEngine;
using UnityEngine.UI;

public class UIFireResolve : CanvasPanel
{
    [Bind("Background")] private Image _background;
    [Bind("Progress")] private Image _progress;

    private Building _targetBuilding;
    private Incident _fireIncident;
    private FireTruck _fireTruck;

    public override void SetPanelInfo(object Info)
    {
        _fireTruck = Info as FireTruck;
        _targetBuilding = _fireTruck.TargetBuilding;

        _fireIncident = Managers.Event.Fire.IncidentBuildings[_targetBuilding];
    }

    void Update()
    {
        if (_targetBuilding == null)
            return;

        Rect.position = Camera.main.WorldToScreenPoint(_targetBuilding.transform.position) + Vector3.up * 300f;

        if (_fireIncident.State == IncidentState.Resolving)
        {
            _progress.fillAmount = _fireIncident.ResolvingProgress;
        }
        else if (_fireIncident.State == IncidentState.Resolved)
        {
            _fireTruck.Return();
            Close();
        }
    }
}
