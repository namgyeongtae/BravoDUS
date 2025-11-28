using UnityEngine;
using TMPro;

public class CityUI : MonoBehaviour
{
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI taxRateText;

    public void UpdateUI(CityState city)
    {
        // Text UI
        populationText.text = $"{city.currentPopulation + "/" + city.maxPopulation}";
        happinessText.text = $"{city.happiness}";
        taxRateText.text = $"{city.taxRate + "%"}";
    }
}
