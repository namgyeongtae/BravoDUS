using Cysharp.Threading.Tasks;
using UnityEngine;

public class UITitle : CanvasPanel
{
    private Animator _animator;


    protected override void Start()
    {
        base.Start();

        _animator = GetComponent<Animator>();

        UIButton startButton = GetComponentInChildren<UIButton>();
        startButton.BindEvent(async () => await OnStartButtonClicked(), ClickType.Up);
    }

    public async UniTask OnStartButtonClicked()
    {
        Debug.Log("OnStartButtonClicked");

        _animator.SetTrigger("Fade");

        await Managers.Level.LoadSceneAsync("MainScene");

        GameManager.Instance.Managers.Init();
    }
}
