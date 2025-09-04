using UnityEngine;
using UnityEngine.UI;

public class SceneUI : CanvasPanel
{
    [Bind("SliderImage")] private Image _levelGaugeSlider;
    [Bind("HomeButton")] private UIButton _homeButton;
    [Bind("Level")] private Text _levelText;

    [Bind("WoodAmount")] private Text _woodAmount;
    [Bind("IronAmount")] private Text _ironAmount;

    [Bind("SettingButton")] private UIButton _settingButton;
    [Bind("QuestionButton")] private UIButton _questionButton;
    [Bind("MenuButton")] private UIButton _menuButton;

    [Bind("UIBuildingSelection")]private UIBuildingSelection _buildingSelection;

    public UIBuildingSelection BuildingSelection => _buildingSelection;

    protected override void Initialize()
    {
        base.Initialize();

        Debug.Log("SceneUI Initialize");

        BindEvent(_homeButton, OnShopButtonClicked);
    }

    public override void Open()
    {
        // _buildingSelection = Managers.UI.AddPanel<UIBuildingSelection>();
    }

    private void OnShopButtonClicked()
    {
        // TODO
        // ī�޶� ���� ������ �����̰� ���� UI ����
    }

    public void AddCommodity(Ingredient ingredient, float amount)
    {
        IngredientType type = ingredient.Type;

        switch (type)
        {
            case IngredientType.Wood:
                _woodAmount.text = amount.ToString();
                break;
            case IngredientType.Iron:
                _ironAmount.text = amount.ToString();
                break;
        }
    }

    // 빌딩 선택 창 열기 혹은 닫기
    public void ToggleBuildingSelection(BuildingType type)
    {
        bool isOpen = _buildingSelection.IsOpen;

        if (isOpen)
        {
            _buildingSelection.DespawnButtons();
        }
        else
        {
            _buildingSelection.SpawnButtons(type);
        }
    }
}
