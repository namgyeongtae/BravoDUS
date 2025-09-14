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
    [Bind("WoodAmount")] private Text _woodAmount;
    [Bind("IronAmount")] private Text _ironAmount;
    private Coroutine _woodCoroutine = null;
    private Coroutine _ironCoroutine = null;

    [Header("SideGroup")]
    [Bind("HomeButton")] private UIButton _homeButton;
    [Bind("SettingButton")] private UIButton _settingButton;
    [Bind("QuestionButton")] private UIButton _questionButton;
    [Bind("MenuButton")] private UIButton _menuButton;

    [Header("Build")]
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
        _woodAmount.text = Managers.Commodity.GetIngredient(IngredientType.Wood).Amount.ToString();
        _ironAmount.text = Managers.Commodity.GetIngredient(IngredientType.Iron).Amount.ToString();
    }

    private void OnShopButtonClicked()
    {
        // TODO
        // ī�޶� ���� ������ �����̰� ���� UI ����
    }
    private IEnumerator CoAddCommodity(Ingredient ingredient, float amount)
    {
        IngredientType type = ingredient.Type;

        float currentAmount = type switch
        {
            IngredientType.Wood => Convert.ToSingle(_woodAmount.text),
            IngredientType.Iron => Convert.ToSingle(_ironAmount.text),
            _ => 0f
        };

        while (currentAmount < amount)
        {
            currentAmount += 1;
            yield return new WaitForSeconds(0.01f);

            switch (type)
            {
                case IngredientType.Wood:
                    _woodAmount.text = currentAmount.ToString();
                    break;
                case IngredientType.Iron:
                    _ironAmount.text = currentAmount.ToString();
                    break;
            }
        }
    }

    private IEnumerator CoSubCommodity(Ingredient ingredient, float amount)
    {
        IngredientType type = ingredient.Type;

        float currentAmount = type switch
        {
            IngredientType.Wood => Convert.ToSingle(_woodAmount.text),
            IngredientType.Iron => Convert.ToSingle(_ironAmount.text),
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
                    _ironAmount.text = currentAmount.ToString();
                    break;
            }
        }
    }

    public void AddCommodity(Ingredient ingredient, float amount)
    {
        switch (ingredient.Type)
        {
            case IngredientType.Wood:
                if (_woodCoroutine != null)
                {
                    StopCoroutine(_woodCoroutine);
                    _woodCoroutine = null;
                }
                _woodCoroutine = StartCoroutine(CoAddCommodity(ingredient, amount));
                break;
            case IngredientType.Iron:
                if (_ironCoroutine != null)
                {
                    StopCoroutine(_ironCoroutine);
                    _ironCoroutine = null;
                }
                _ironCoroutine = StartCoroutine(CoAddCommodity(ingredient, amount));
                break;
        }
    }

    public void SubCommodity(Ingredient ingredient, float amount)
    {
        switch (ingredient.Type)
        {
            case IngredientType.Wood:
                if (_woodCoroutine != null)
                {
                    StopCoroutine(_woodCoroutine);
                    _woodCoroutine = null;
                }
                _woodCoroutine = StartCoroutine(CoSubCommodity(ingredient, amount));
                break;
            case IngredientType.Iron:
                if (_ironCoroutine != null)
                {
                    StopCoroutine(_ironCoroutine);
                    _ironCoroutine = null;
                }
                _ironCoroutine = StartCoroutine(CoSubCommodity(ingredient, amount));
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
