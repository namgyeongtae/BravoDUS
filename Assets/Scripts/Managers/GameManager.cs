using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Managers _managers = new Managers();

    private void Awake()
    {
        Instance = this;

        _managers.Init();

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        _managers.Update();
    }

    private void OnDestroy()
    {
        _managers.Release();
    }
}
