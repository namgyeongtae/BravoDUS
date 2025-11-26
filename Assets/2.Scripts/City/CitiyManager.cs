using UnityEngine;

public class CityManager : MonoBehaviour
{
    public static CityManager Instance { get; private set; }

    public CityState city = new CityState();

    public CityUI cityUI;
    public PopulationSystem populationSystem;
    public MoneySystem moneySystem;
    public HappinessSystem happinessSystem;
    public TaxSystem taxSystem;
    public DateSystem dateSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        UpdateCityState();
        cityUI.UpdateUI(city);
    }

    void UpdateCityState()
    {
        city.currentPopulation = populationSystem.GetCurrentPopulation();
        city.maxPopulation = populationSystem.GetMaxPopulation();
        city.money = moneySystem.GetMoney();
        city.happiness = happinessSystem.GetHappiness();
        city.taxRate = taxSystem.GetTaxRate();
    }
}
