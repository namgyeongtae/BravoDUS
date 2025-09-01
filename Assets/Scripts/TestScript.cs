using UnityEngine;
using UnityEngine.EventSystems;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Managers.UI.GetUI<SceneUI>("SceneUI").ToggleBuildingSelection(BuildingType.Center);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            var buildButtonGroup = Managers.UI.GetUI<UIBuildButtonGroup>("UIBuildButtonGroup");

            if (buildButtonGroup == null)
            {
                Managers.UI.AddPanel<UIBuildButtonGroup>("UIBuildButtonGroup"); 
            }
            else
            {
                buildButtonGroup.Close();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ui = Managers.UI.GetUI<UIBuildButtonGroup>("UIBuildButtonGroup");
            if (ui != null)
            {
                // Input.mousePosition이 UI와 겹쳐있는가?
                if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
                {
                    ui.Close();
                }
            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Building")))
                {
                    Debug.Log("Raycast hit");
                    var go = hit.collider.gameObject;
                    Managers.UI.AddPanel<UIBuildButtonGroup>("UIBuildButtonGroup", go);
                }
            }
        }
    }
}
