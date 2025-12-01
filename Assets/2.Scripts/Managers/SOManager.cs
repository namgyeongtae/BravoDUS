using UnityEngine;

public class SOManager : IManagerBase
{
    public BuildingSO BuildingSO => Managers.Resource.LoadSO<BuildingSO>("BuildingSO");
}
