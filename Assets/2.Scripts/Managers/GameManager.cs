using UnityEngine;

[DefaultExecutionOrder(1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Managers _managers = new Managers();

    public Managers Managers => _managers;

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