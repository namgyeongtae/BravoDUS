using UnityEngine;
using UnityEngine.UI;
public class UIBuildingSlot : UIBind
{
    [Bind("Background")] private UIButton _slotButton;
    [Bind("BuildingImage")] private Image _buildingImage;
    [Bind("NameText")] private Text _nameText;

    private GameObject _buildingPrefab;
    private int _buildingSize;

    public override void Open()
    {
        base.Initialize();

        Debug.Log("Building Slot Init");

        _slotButton.BindEvent(OnClickSlotButton, ClickType.Up);
    }

    public void SetSlot(BuildingData buildingData)
    {
        _buildingImage.sprite = buildingData.buildingIcon;
        _nameText.text = buildingData.buildingName;
        _buildingPrefab = buildingData.buildingPrefab;
        _buildingSize = buildingData.buildingSize;
    }

    private void OnClickSlotButton()
    {
        Debug.Log("OnClickSlotButton");

        // 자원 조건 체크
        if (!CraftingManager.Instance.CheckResources(_buildingPrefab.GetComponent<Building>(), 1))
        {
            var toast = Managers.UI.AddPanel<UIToastPopup>();
            toast.SettingPopup("자원이 부족해서 건설할 수 없습니다.");
            return;
        }

        Managers.Construct.SwitchConstructMode(ConstructMode.Placement);

        Managers.UI.AddPanel<UIBuildPlacement>(_buildingPrefab);
        Managers.Construct.PlacementSystem.StartPlacement(_buildingPrefab, _buildingSize);

        Managers.UI.GetUI<UIBuildingMenu>().Close();
    }
}
