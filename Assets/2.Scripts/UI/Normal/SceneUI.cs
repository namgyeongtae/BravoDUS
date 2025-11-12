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
    // [Bind("IronIcon")] private UIParticleAttractor _ironParticleAttractor;
    [Bind("WoodAmount")] private Text _woodAmount;
    // [Bind("IronAmount")] private Text _ironAmount;
    private Coroutine _woodAddCoroutine = null;
    private Coroutine _ironAddCoroutine = null;
    private Coroutine _woodSubCoroutine = null;
    private Coroutine _ironSubCoroutine = null;
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
    // public UIParticleAttractor IronParticleAttractor => _ironParticleAttractor;


    protected override void Initialize()
    {
        base.Initialize();

        BindEvent(_buildButton, OnBuildButtonClicked);
        BindEvent(_roadButton, OnRoadButtonClicked);
    }

    public override void Open()
    {
        // _buildingSelection = Managers.UI.AddPanel<UIBuildingSelection>();
        _woodAmount.text = Managers.Commodity.GetIngredient(IngredientType.Wood).Amount.ToString();
        
        // _ironAmount.text = Managers.Commodity.GetIngredient(IngredientType.Iron).Amount.ToString();
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
    private IEnumerator CoAddCommodity(Ingredient ingredient, float amount)
    {
        IngredientType type = ingredient.Type;

        float currentAmount = type switch
        {
            IngredientType.Wood => Convert.ToSingle(_woodAmount.text),
            // IngredientType.Iron => Convert.ToSingle(_ironAmount.text),
            _ => 0f
        };

        while (currentAmount < amount)
        {
            currentAmount += 1;
            Debug.Log($"AddCommodity : {currentAmount} / {amount}");
            yield return new WaitForSeconds(0.01f);

            switch (type)
            {
                case IngredientType.Wood:
                    _woodAmount.text = currentAmount.ToString();
                    break;
                case IngredientType.Iron:
                    // _ironAmount.text = currentAmount.ToString();
                    break;
            }
        }

        if (ingredient.Type == IngredientType.Wood)
        {
            _woodAddCoroutine = null;
        }
        else if (ingredient.Type == IngredientType.Iron)
        {
            _ironAddCoroutine = null;
        }
    }

    private IEnumerator CoSubCommodity(Ingredient ingredient, float amount)
    {
        IngredientType type = ingredient.Type;

        float currentAmount = type switch
        {
            IngredientType.Wood => Convert.ToSingle(_woodAmount.text),
            // IngredientType.Iron => Convert.ToSingle(_ironAmount.text),
            _ => 0f
        };

        while (currentAmount > amount)
        {
            currentAmount -= 1;
            yield return new WaitForSeconds(0.01f);

            switch (type)
            {
                case IngredientType.Wood:
                    _woodAmount.text = currentAmount.ToString();
                    break;
                case IngredientType.Iron:
                    // _ironAmount.text = currentAmount.ToString();
                    break;
            }
        }

        if (ingredient.Type == IngredientType.Wood)
        {
            _woodSubCoroutine = null;
        }
        else if (ingredient.Type == IngredientType.Iron)
        {
            _ironSubCoroutine = null;
        }
    }

    public void AddCommodity(Ingredient ingredient, float amount)
    {
        switch (ingredient.Type)
        {
            case IngredientType.Wood:
                /* if (_woodCoroutine != null)
                {
                    StopCoroutine(_woodCoroutine);
                    _woodCoroutine = null;
                } */
                if (_woodAddCoroutine == null)
                    _woodAddCoroutine = StartCoroutine(CoAddCommodity(ingredient, amount));
                break;
            case IngredientType.Iron:
                /* if (_ironCoroutine != null)
                {
                    StopCoroutine(_ironCoroutine);
                    _ironCoroutine = null;
                } */
                if (_ironAddCoroutine == null)
                    _ironAddCoroutine = StartCoroutine(CoAddCommodity(ingredient, amount));
                break;
        }
    }

    public void SubCommodity(Ingredient ingredient, float amount)
    {
        switch (ingredient.Type)
        {
            case IngredientType.Wood:
                /* if (_woodSubCoroutine != null)
                {
                    StopCoroutine(_woodSubCoroutine);
                    _woodSubCoroutine = null;
                } */
                _woodSubCoroutine = StartCoroutine(CoSubCommodity(ingredient, amount));
                break;
            case IngredientType.Iron:
                /* if (_ironSubCoroutine != null)
                {
                    StopCoroutine(_ironSubCoroutine);
                    _ironSubCoroutine = null;
                } */
                _ironSubCoroutine = StartCoroutine(CoSubCommodity(ingredient, amount));
                break;
        }
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
