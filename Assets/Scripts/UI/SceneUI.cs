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
        // _levelText.text = $"{현재 정부 시설의 레벨}"
        // _levelGaugeSlider.fillAmount = {현재 유저의 충전 게이지} / {다음 레벨로의 필요 게이지}
    }

    private void OnShopButtonClicked()
    {
        // TODO
        // 카메라를 슈퍼 쪽으로 움직이고 상점 UI 오픈
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
