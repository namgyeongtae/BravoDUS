using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class PopulationSystem : MonoBehaviour
{
    [SerializeField] GameObject warningIcon;
    [SerializeField] Image guage;

    public int currentPopulation = 1;
    public int maxPopulation = 1;

    bool isWarning = false;

    public event Action OnPopulationChanged;

    void Update()
    {
        Mathf.Clamp(currentPopulation, 0, CityManager.Instance.happinessSystem.GetHappiness());

        Warning();
        Guage();
    }

    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }

    public int GetMaxPopulation()
    {
        return maxPopulation;
    }

    public void ApplyPopulationChange(int delta)
    {
        OnPopulationChanged?.Invoke();

        currentPopulation += delta;
        maxPopulation += delta;
    }

    void Warning()
    {
        if (maxPopulation > CityManager.Instance.city.happiness && !isWarning)
        {
            isWarning = true;
            warningIcon.SetActive(true);
            guage.color = new Color32(255, 69, 0, 255);
        }

        else if (maxPopulation < CityManager.Instance.city.happiness && isWarning)
        {
            isWarning = false;
            warningIcon.SetActive(false);
            guage.color = new Color32(255, 255, 255, 255);
        }
    }

    void Guage()
    {
        if (maxPopulation > currentPopulation)
        {
            guage.fillAmount = (float)currentPopulation / (float)maxPopulation;
        }
    }
}
