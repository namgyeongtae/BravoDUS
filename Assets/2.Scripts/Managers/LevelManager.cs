using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;


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
        await UniTask.Delay(1000);
        await FadeIn();
        
         // 어드레서블 씬 로드
        var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        
        // 진행률이 90% 이상이 될 때까지 대기
        while (handle.PercentComplete < 0.9f)
        {
            await UniTask.Yield();
        }

        // 씬 활성화 및 페이드 아웃
        await FadeOut(handle);

        if (sceneName == "MainScene")
        {
            GameManager.Instance.SetGameStarted(true);
        }
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

    public async UniTask FadeOut(AsyncOperationHandle<SceneInstance> handle)
    {
        await handle.Task;

        var fadeSceneTransition = Managers.UI.GetUI<FadeSceneTransition>("FadeSceneTransition");

        if (fadeSceneTransition != null)
        {
            Managers.UI.RemovePanel(fadeSceneTransition);
        }

        fadeSceneTransition = Managers.UI.AddPanel<FadeSceneTransition>("FadeSceneTransition");
        await fadeSceneTransition.AnimateTransitionOut();
    }
}
