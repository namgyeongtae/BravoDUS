using UnityEngine;
using TMPro;

public class CityUI : MonoBehaviour
{
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI happinessText;
    // public TextMeshProUGUI moneyText;
    public TextMeshProUGUI taxRateText;

    public void UpdateUI(CityState city)
    {
        populationText.text = $"{city.currentPopulation + "/" + city.maxPopulation}";
        happinessText.text = $"{city.happiness}";
        // moneyText.text = $"{city.money}";
        taxRateText.text = $"{city.taxRate + "%"}";
    }
}
