using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingMenu : CanvasPanel
{
    [SerializeField] private BuildingSO _buildingSO;

    [Bind("CloseButton")] private UIButton _closeButton;
    [Bind("BuildingScroll")] private ScrollRect _buildingScroll;

    protected override void Initialize()
    {
        base.Initialize();
        CreateBuildingSlots();

        _closeButton.BindEvent(() => Close(), ClickType.Up);
    }

    public override void Open()
    {
        base.Open();

        
    }

    private void CreateBuildingSlots()
    {
        foreach (var buildingData in _buildingSO.buildingDatas)
        {
            var slot = Managers.Resource.Instantiate("UI/UIBuildingSlot").GetComponent<UIBuildingSlot>();
            slot.transform.SetParent(_buildingScroll.content);
            slot.transform.localScale = Vector3.one;
            slot.SetSlot(buildingData);
        }
    }
}
