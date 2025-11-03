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
    private PlacementSystem _placementSystem;
    private RoadSystem _roadSystem;

    public PlacementSystem PlacementSystem => _placementSystem;
    public RoadSystem RoadSystem => _roadSystem;

    public ConstructMode ConstructMode => _constructMode;

    public void Init()
    {
        _placementSystem = GameObject.FindFirstObjectByType<PlacementSystem>();
        _roadSystem = GameObject.FindFirstObjectByType<RoadSystem>();
    }

    public void SwitchConstructMode(ConstructMode mode = ConstructMode.None)
    {
        _constructMode = mode;
    }
}
