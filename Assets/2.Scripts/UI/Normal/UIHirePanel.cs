using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class UIHirePanel : CanvasPanel
{
    [Header("Tab Menu")]
    [Bind("WoodWorker")] private UIButton _woodWorkerButton;
    [Bind("IronWorker")] private UIButton _ironWorkerButton;
    [Bind("Doctor")] private UIButton _doctorButton;
    [Bind("Firefighter")] private UIButton _firefighterButton;

    [Bind("JobScroll")] private ScrollRect _jobScroll;
    [Bind("CloseButton")] private UIButton _closeButton;

    private List<UIWorkerSlot> _workerSlots = new();
    private JobType _currentTab = JobType.WoodWorker;
    private Dictionary<JobType, UIButton> _tabButtonDict = new();

    private Color _originTabColor;

    protected override void Initialize()
    {
        _woodWorkerButton.onClickUp += () => { OnClickTabButton(JobType.WoodWorker); };
        _ironWorkerButton.onClickUp += () => { OnClickTabButton(JobType.IronWorker); };
        _doctorButton.onClickUp += () => { OnClickTabButton(JobType.Doctor); };
        _firefighterButton.onClickUp += () => { OnClickTabButton(JobType.FireFighter); };

        _closeButton.BindEvent(Close, ClickType.Up);

        _tabButtonDict.Add(JobType.WoodWorker, _woodWorkerButton);
        _tabButtonDict.Add(JobType.IronWorker, _ironWorkerButton);
        _tabButtonDict.Add(JobType.Doctor, _doctorButton);
        _tabButtonDict.Add(JobType.FireFighter, _firefighterButton);
    }

    public override void Open()
    {
        base.Open();
        UpdateTab(_currentTab);
    }

    private void OnClickTabButton(JobType tab)
    {
        if (_currentTab == tab)
            return;

        UpdateTab(tab);

        // TODO
        // _tabButtonDict[_currentTab].GetComponent<Image>().color = _originTabColor;
        // _tabButtonDict[tab].GetComponent<Image>().color = new Color();

        _currentTab = tab;
    }

    private void UpdateTab(JobType tab)
    {
        foreach (var slot in _workerSlots)
        {
            Managers.Resource.Destroy(slot.gameObject);
        }
        _workerSlots.Clear();

        var jobList = Managers.HR.WorkForceDictionary[tab];
        foreach (var job in jobList)
        {
            var slot = Managers.Resource.Instantiate("UI/UIWorkerSlot").GetComponent<UIWorkerSlot>();
            slot.transform.SetParent(_jobScroll.content);
            slot.transform.localScale = Vector3.one;
            slot.SetSlot(job);
            
            _workerSlots.Add(slot);
        }
    }
}
