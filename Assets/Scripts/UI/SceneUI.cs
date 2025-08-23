using UnityEngine;
using UnityEngine.UI;

public class SceneUI : CanvasPanel
{
    [Bind("SliderImage")] private Image _levelGaugeSlider;
    [Bind("ShopButton")] private Button _shopButton;
    [Bind("Level")] private Text _levelText;

    [Bind("WoodAmount")] private Text _woodAmount;
    [Bind("IronAmount")] private Text _ironAmount;

    [Bind("SettingButton")] private Button _settingButton;

    protected override void Initialize()
    {
        base.Initialize();

        Debug.Log("SceneUI Initialize");

        BindEvent(_shopButton, OnShopButtonClicked);
    }

    public override void Open()
    {
        // _levelText.text = $"{���� ���� �ü��� ����}"
        // _levelGaugeSlider.fillAmount = {���� ������ ���� ������} / {���� �������� �ʿ� ������}
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
}
