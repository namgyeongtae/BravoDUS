using UnityEngine;

public class UIPatientManage : CanvasPanel
{
    [Bind("Content")] private UIPatientSlot[] _patientSlots;
    [Bind("CloseButton")] private UIButton _closeButton;

    private HospitalRole _hospital;

    protected override void Initialize()
    {
        base.Initialize();
        _closeButton.BindEvent(Close, ClickType.Up);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void SetPanelInfo(object info)
    {
        _hospital = (info as Building).GetComponent<HospitalRole>();

        Debug.Log("Should after start bind");

        var injureEventController = Managers.Event.Injure as InjureEventController;
        for (int i = 0; i < _hospital.InjurePatients.Count; i++)
        {
            var inc = _hospital.InjurePatients[i];
            WorkForce wf = injureEventController.IncidentWorkForces[inc];

            Debug.Log("Should after start bind");
            
            _patientSlots[i].SetSlot(wf);
        }
    }
}
