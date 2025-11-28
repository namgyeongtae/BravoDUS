using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
public class UITitle : CanvasPanel
{
    private Animator _animator;

    [SerializeField] private GameObject _downloadPanel;
    [SerializeField] private Image _downloadProgress;
    [SerializeField] private Text _downloadProgressText;


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

        // 이미 다운 받았는지 확인
        var downloadSize = await Addressables.GetDownloadSizeAsync("default");
        if(downloadSize > 0)
        {
            await DownloadAsync("default");
        }
        
        await Managers.Level.LoadSceneAsync("MainScene");
        
        _animator.SetTrigger("Fade");


        GameManager.Instance.Managers.Init();
    }

    public async UniTask DownloadAsync(string label)
    {
        _downloadPanel.SetActive(true);
        _downloadProgress.fillAmount = 0;
        _downloadProgressText.text = "0.00 %";

        try 
        {
            var downLoadSizeHandle = Addressables.GetDownloadSizeAsync(label);
            var downLoadSize = await downLoadSizeHandle;

            Addressables.Release(downLoadSizeHandle);

            if(downLoadSize > 0)
            {
                var handle = Managers.CDN.DownloadAsync(label);
                while(!handle.IsDone)
                {
                    var downStatus = handle.GetDownloadStatus();
                    Debug.Log($"{downStatus.DownloadedBytes}/{downStatus.TotalBytes}");
                    Debug.Log($"{downStatus.Percent * 100f} %");

                    _downloadProgress.fillAmount = downStatus.Percent;
                    _downloadProgressText.text = $"{downStatus.Percent * 100f:F2} %";

                    await UniTask.Yield();
                }
                
                Addressables.Release(handle);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"다운로드 사이즈 확인 실패 (라벨: {label}): {e.Message}");
            Debug.LogError($"에러 타입: {e.GetType().Name}");
            throw;
        }
    }
}
