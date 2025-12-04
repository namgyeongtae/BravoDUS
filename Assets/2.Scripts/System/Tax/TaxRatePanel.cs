using UnityEngine;

public class TaxRatePanel : MonoBehaviour
{

    private void Start()
    {
        gameObject.SetActive(false);
    }
    public void ActivatePanel()
    {
        gameObject.SetActive(true);
    }

    public void DeActivatePanel()
    {
        gameObject.SetActive(false);
    }


}
