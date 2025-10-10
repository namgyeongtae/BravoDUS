using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkerSlot : UIBind
{
    [Bind("WorkerIcon")] private Image _workerIcon;
    [Bind("WorkerName")] private Text _workerName;
    [Bind("JobName")] private Text _jobName;
    [Bind("HireButton")] private UIButton _hireButton;
    [Bind("HiredImage")] private Image _hiredImage;

    private WorkForce _workForce;

    public WorkForce WorkForce => _workForce;

    public override void Open()
    {
        base.Open();
        _hireButton.BindEvent(OnClickHireButton, ClickType.Up);
    }

    public void SetSlot(WorkForce workForce)
    {
        _workForce = workForce;
        _workerIcon.sprite = AtlasController.GetSprite(workForce.Icon, workForce.Icon.Split('/').Last() + $"_{(int)workForce.JobType}");
        _workerName.text = workForce.Name;
        _jobName.text = workForce.JobType.ToString();

        bool isHired = Managers.HR.HoldResources.Contains(workForce);

        _hireButton.gameObject.SetActive(!isHired);
        _hiredImage.gameObject.SetActive(isHired);
    }

    private void OnClickHireButton()
    {
        if (_workForce == null)
        {
            Debug.LogError("WorkForce is null");
            return;
        }
        Managers.HR.HireWorkForce(_workForce);

        _hireButton.gameObject.SetActive(false);
        _hiredImage.gameObject.SetActive(true);
    }
}
