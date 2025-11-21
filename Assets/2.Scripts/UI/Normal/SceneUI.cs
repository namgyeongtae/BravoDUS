using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneUI : CanvasPanel
{
    [Header("UserInfo")]
    [Bind("SliderImage")] private Image _levelGaugeSlider;
    [Bind("Level")] private Text _levelText;

    [Header("Commodity")]
    [Bind("WoodIcon")] private UIParticleAttractor _woodParticleAttractor;
    [Bind("WoodAmount")] private Text _woodAmount;
    private Coroutine _woodAddCoroutine = null;
    private Coroutine _woodSubCoroutine = null;
    
    [Header("SideGroup")]
    [Bind("BuildButton")] private UIButton _buildButton;
    [Bind("RoadButton")] private UIButton _roadButton;
    [Bind("SettingButton")] private UIButton _settingButton;
    [Bind("QuestionButton")] private UIButton _questionButton;
    [Bind("MenuButton")] private UIButton _menuButton;

    [Header("Build")]
    [Bind("UIBuildingSelection")] private UIBuildingSelection _buildingSelection;

    public UIBuildingSelection BuildingSelection => _buildingSelection;

    public UIParticleAttractor WoodParticleAttractor => _woodParticleAttractor;


    protected override void Initialize()
    {
        base.Initialize();

        BindEvent(_buildButton, OnBuildButtonClicked);
        BindEvent(_roadButton, OnRoadButtonClicked);
    }

    public override void Open()
    {
        _woodAmount.text = Managers.Commodity.Money.ToString();
    }

    private void OnBuildButtonClicked()
    {
        Managers.UI.AddPanel<UIBuildingMenu>();
    }

    private void OnRoadButtonClicked()
    {
        Managers.Construct.SwitchConstructMode(ConstructMode.Road);
        Managers.UI.AddPanel<UIRoadPlacement>();
    }
    private IEnumerator CoAddCommodity(float amount)
    {
        float currentAmount = Convert.ToSingle(_woodAmount.text);

        while (currentAmount < amount)
        {
            currentAmount += 1;
            yield return new WaitForSeconds(0.01f);

            _woodAmount.text = currentAmount.ToString();
        }
    }

    private IEnumerator CoSubCommodity(float amount)
    {
        float currentAmount = Convert.ToSingle(_woodAmount.text);

        while (currentAmount > amount)
        {
            currentAmount -= 1;
            yield return new WaitForSeconds(0.01f);

            _woodAmount.text = currentAmount.ToString();
        }
    }

    public void AddCommodity(float amount)
    {
        if (_woodAddCoroutine != null)
        {
            StopCoroutine(_woodAddCoroutine);
            _woodAddCoroutine = null;
        }

        _woodAddCoroutine = StartCoroutine(CoAddCommodity(amount));
    }

    public void SubCommodity(float amount)
    {
        if (_woodSubCoroutine != null)
        {
            StopCoroutine(_woodSubCoroutine);
            _woodSubCoroutine = null;
        }
        _woodSubCoroutine = StartCoroutine(CoSubCommodity(amount));
    }

    // 빌딩 선택 창 열기 혹은 닫기
    public void ToggleBuildingSelection(Building building)
    {
        bool isOpen = _buildingSelection.IsOpen;

        _buildingSelection.SetSelectedBuilding(building);

        if (isOpen)
        {
            _buildingSelection.DespawnButtons();
        }
        else
        {
            _buildingSelection.SpawnButtons();
        }
    }

    public void AddParticleToAttractor(IngredientType type, UIParticle particle)
    {
        switch (type)
        {
            case IngredientType.Wood:
                _woodParticleAttractor.AddParticle(particle);
                break;
            case IngredientType.Iron:
                // _ironParticleAttractor.AddParticle(particle);
                break;
        }
    }
}
