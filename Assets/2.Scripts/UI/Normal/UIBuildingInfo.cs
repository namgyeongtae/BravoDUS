using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingInfo : CanvasPanel
{
    [Bind("BuildingImage")] private Image _buildingImage;
    [Bind("NameText")] private TextMeshProUGUI _nameText;
    [Bind("HappinessText")] private TextMeshProUGUI _happinessText;
    [Bind("PopulationText")] private TextMeshProUGUI _populationText;
    [Bind("DescriptionText")] private TextMeshProUGUI _descriptionText;
    [Bind("CloseButton")] private UIButton _closeButton;

    private Building _targetBuilding = null;

    protected override void Initialize()
    {
        _closeButton.BindEvent(Close, ClickType.Up);
    }

    public override void SetPanelInfo(object Info)
    {
        _targetBuilding = Info as Building;

        UpdateBuildingInfo();
    }

    private void UpdateBuildingInfo()
    {
        if (_targetBuilding == null)
            return;

        var buildingData = Managers.SO.BuildingSO.buildingDatas.Find(x => x.buildingType == _targetBuilding.BuildingType);

        _buildingImage.sprite = buildingData.buildingIcon;
        _nameText.text = buildingData.buildingType.ToString();
        _happinessText.text = buildingData.Happiness.ToString();
        _populationText.text = buildingData.Population.ToString();
        // _descriptionText.text = buildingData.description;
    }
}
