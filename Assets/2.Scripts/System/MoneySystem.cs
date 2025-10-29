using UnityEngine;
using TMPro;

public class MoneySystem : MonoBehaviour
{
    int money;

    public void CollectTax(int tax)
    {
        money += tax;
    }

    public int GetMoney()
    {
        return money;
    }
}
