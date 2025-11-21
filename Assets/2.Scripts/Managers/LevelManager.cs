using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelManager : IManagerBase
{
    public void Init()
    {
        
    }

    public void Update()
    {
        
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        await FadeIn();
        await UniTask.Delay(1000);

        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
        {
            await UniTask.Yield();
        }

        asyncOperation.allowSceneActivation = true;
        await FadeOut(asyncOperation);

        GameManager.Instance.Managers.Init();
    }

    public async UniTask FadeIn()
    {
        var fadeSceneTransition = Managers.UI.GetUI<FadeSceneTransition>("FadeSceneTransition");

        if (fadeSceneTransition != null)
        {
            Managers.UI.RemovePanel(fadeSceneTransition);
        }

        fadeSceneTransition = Managers.UI.AddPanel<FadeSceneTransition>("FadeSceneTransition");

        await fadeSceneTransition.AnimateTransitionIn();
    }

    public async UniTask FadeOut(AsyncOperation asyncOperation = null)
    {
        asyncOperation.allowSceneActivation = true;

        await UniTask.WaitUntil(() => asyncOperation.isDone);

        var fadeSceneTransition = Managers.UI.GetUI<FadeSceneTransition>("FadeSceneTransition");

        if (fadeSceneTransition != null)
        {
            Managers.UI.RemovePanel(fadeSceneTransition);
        }

        fadeSceneTransition = Managers.UI.AddPanel<FadeSceneTransition>("FadeSceneTransition");
        await fadeSceneTransition.AnimateTransitionOut();
    }
}
