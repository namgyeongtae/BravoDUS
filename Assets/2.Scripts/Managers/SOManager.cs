using UnityEngine;

public class SOManager : IManagerBase
{
    private BuildingSO _buildingSO;

    public BuildingSO BuildingSO => _buildingSO;

    public void Init()
    {
        _buildingSO = Managers.Resource.LoadSO<BuildingSO>("BuildingSO");
    }
}
