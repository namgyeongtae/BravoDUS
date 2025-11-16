using UnityEngine;
using TMPro;

public class CityUI : MonoBehaviour
{
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI moneyText;

    public void UpdateUI(CityState city)
    {
        populationText.text = $"{city.population}";
        happinessText.text = $"{city.happiness}";
        moneyText.text = $"{city.money}";
    }
}
