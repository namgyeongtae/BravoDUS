using UnityEngine;

public class CitiyManager : MonoBehaviour
{
    public CityState city = new CityState();

    public PopulationSystem populationSystem;
    public MoneySystem moneySystem;
    public HappinessSystem happinessSystem;
    public CityUI cityUI;

    private void Update()
    {
        UpdateCityState();
        cityUI.UpdateUI(city);
    }

    void UpdateCityState()
    {
        city.population = populationSystem.GetPopulation();
        city.money = moneySystem.GetMoney();
        city.happiness = happinessSystem.GetHappiness();
    }
}
