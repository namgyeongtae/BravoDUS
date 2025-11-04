using UnityEngine;

public enum ConstructMode
{
    None,
    Placement,
    Road
}

public class ConstructManager : IManagerBase
{
    private ConstructMode _constructMode = ConstructMode.None;
    private GridHandler _gridHandler;
    private PlacementSystem _placementSystem;
    private RoadSystem _roadSystem;

    public GridHandler GridHandler => _gridHandler;
    public PlacementSystem PlacementSystem => _placementSystem;
    public RoadSystem RoadSystem => _roadSystem;

    public ConstructMode ConstructMode => _constructMode;

    public void Init()
    {
        _placementSystem = GameObject.FindFirstObjectByType<PlacementSystem>();
        _roadSystem = GameObject.FindFirstObjectByType<RoadSystem>();
        _gridHandler = GameObject.FindFirstObjectByType<GridHandler>();
    }

    public void SwitchConstructMode(ConstructMode mode = ConstructMode.None)
    {
        if (mode != ConstructMode.None)
        {
            Managers.UI.GetUI<SceneUI>().gameObject.SetActive(false);
            _gridHandler.EnterBuildMode();
        }
        else
        {
            Managers.UI.GetUI<SceneUI>().gameObject.SetActive(true);
            _gridHandler.ExitBuildMode();
        }
        
        _constructMode = mode;
    }
}
