using UnityEngine;
using UnityEngine.SceneManagement;

public enum ConstructMode
{
    None,
    Placement,
    Road
}

public class ConstructManager : IManagerBase
{
    private ConstructMode _constructMode = ConstructMode.None;
    /* private GridHandler _gridHandler;
    private PlacementSystem _placementSystem;
    private RoadSystem _roadSystem; */

    public GridHandler GridHandler { get { return GameObject.FindFirstObjectByType<GridHandler>(); } }
    public PlacementSystem PlacementSystem { get { return GameObject.FindFirstObjectByType<PlacementSystem>(); } }
    public RoadSystem RoadSystem { get { return GameObject.FindFirstObjectByType<RoadSystem>(); } }

    public ConstructMode ConstructMode => _constructMode;

    public void Init()
    {
        /* _placementSystem = GameObject.FindFirstObjectByType<PlacementSystem>();
        _roadSystem = GameObject.FindFirstObjectByType<RoadSystem>();
        _gridHandler = GameObject.FindFirstObjectByType<GridHandler>(); */

        Debug.Log($"ConstructManager Init {SceneManager.GetActiveScene().name}");
    }

    public void SwitchConstructMode(ConstructMode mode = ConstructMode.None)
    {
        if (mode != ConstructMode.None)
        {
            Managers.UI.GetUI<SceneUI>().gameObject.SetActive(false);
            GridHandler.EnterBuildMode();
        }
        else
        {
            Managers.UI.GetUI<SceneUI>().gameObject.SetActive(true);
            GridHandler.ExitBuildMode();
        }
        
        _constructMode = mode;
    }
}
