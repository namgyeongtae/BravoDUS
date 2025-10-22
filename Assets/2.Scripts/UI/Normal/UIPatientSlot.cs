using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIPatientSlot : UIBind
{
    public enum PatientSlotState
    {
        Locked,
        Unassigned,
        Assigned
    }

    [Bind("LockSlot")] private GameObject _lockSlot;
    [Bind("AssignSlot")] private UIButton _assignSlot;
    [Bind("PatientSlot")] private GameObject _patientSlot;

    [Bind("PatientIcon")] private Image _patientIcon;
    [Bind("PatientName")] private Text _patientName;
    [Bind("HealProgress")] private Slider _healProgress;

    private WorkForce _assignedPatient;
    private Incident _assignedIncident;
    private PatientSlotState _state;

    public PatientSlotState State
    {
        get { return _state; }
        set
        {
            _state = value;

            switch (_state)
            {
                case PatientSlotState.Locked:
                    _lockSlot.SetActive(true);
                    _assignSlot.gameObject.SetActive(false);
                    _patientSlot.SetActive(false);
                    break;
                case PatientSlotState.Unassigned:
                    _lockSlot.SetActive(false);
                    _assignSlot.gameObject.SetActive(true);
                    _patientSlot.SetActive(false);
                    break;
                case PatientSlotState.Assigned:
                    _lockSlot.SetActive(false);
                    _assignSlot.gameObject.SetActive(false);
                    _patientSlot.SetActive(true);
                    break;
            }
        }
    }

    protected override void Start()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        _assignSlot.BindEvent(OnClickAssignSlot, ClickType.Up);
    }

    void Update()
    {
        if (_state == PatientSlotState.Assigned)
        {
            _healProgress.value = _assignedIncident.ResolvingProgress;
            
            if (_healProgress.value >= 1f)
            {
                State = PatientSlotState.Unassigned;
            }
        }
    }

    public void SetSlot(WorkForce patient)
    {
        if (patient == null)
        {
            _assignedPatient = null;
            return;
        }
        State = PatientSlotState.Assigned;

        var hospital = CraftingManager.Instance.Buildings
                                    .FirstOrDefault(x => x.BuildingType == BuildingType.Hospital)
                                    .GetComponent<HospitalRole>();

        var injureEvent = Managers.Event.Injure as InjureEventController;
        var inc = injureEvent.IncidentWorkForces.FirstOrDefault(x => x.Value == patient).Key;

        _assignedIncident = inc;
        _assignedPatient = patient;

        Debug.Log(Managers.HR.HoldResources[0].HRState);

         var subName = patient.Icon.Split('/').Last();
        _patientIcon.sprite = AtlasController.GetSprite(patient.Icon, subName + $"_{(int)patient.JobType}");
        _patientName.text = _assignedPatient.Name;

        
    }

    private void OnClickAssignSlot()
    {
        var hospital = CraftingManager.Instance.Buildings
                                    .FirstOrDefault(x => x.BuildingType == BuildingType.Hospital)
                                    .GetComponent<HospitalRole>();

        var injureEvent = Managers.Event.Injure as InjureEventController;
        var inc = injureEvent.IncidentWorkForces.FirstOrDefault(x => x.Value.HRState == HRState.Injured).Key;

        if (inc == null)
        {
            Debug.LogError("Injured Patient not found");
            return;
        }

        inc.State = IncidentState.Resolving;


        SetSlot(injureEvent.IncidentWorkForces[inc]);

        Managers.HR.UnassignWorkForce(_assignedPatient);
        
        _assignedPatient.SetHRState(HRState.Recovering);

        hospital.InjurePatients.Add(inc);
    }
}
