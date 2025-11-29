using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.Initialization;

public class CDNManager : IManagerBase
{
    public static string CurrentCatalogVersion { get; private set; }
    public static string CurrentCatalogUrl { get; private set; }

    private const string BaseUrl = "https://s3.ap-southeast-2.amazonaws.com/com.bravo.citybuilder";

    public async void Init()
    {
        await InitializeAsync();
    }

    public static async UniTask<AsyncOperationHandle> InitializeAsync()
    {
        string appVersion = Application.version;

        string catalogVersion = GetResourceVersionForApp(appVersion);

        CurrentCatalogVersion = catalogVersion;

        #if UNITY_ANDROID
        string targetFolder = "Android";
        #elif UNITY_STANDALONE_WIN
        string targetFolder = "StandaloneWindows64";
        #else
        string targetFolder = Application.platform.ToString();
        #endif

        CurrentCatalogUrl = $"{BaseUrl}/{targetFolder}/{catalogVersion}/catalog_{catalogVersion}.json";

        var InitializeHandle = Addressables.InitializeAsync();

        InitializeHandle.Completed += op => {
            Addressables.Release(InitializeHandle);
        };

        await InitializeHandle.Task;
        
        var handle = Addressables.LoadContentCatalogAsync(CurrentCatalogUrl, true);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[Addressables] Catalog load failed: {CurrentCatalogUrl}");
        }
        else
        {
            Debug.Log($"[Addressables] Catalog loaded from: {CurrentCatalogUrl}");
        }

        return handle;
    }

    public AsyncOperationHandle DownloadAsync(string label)
    {
        var handle = Addressables.DownloadDependenciesAsync(label);

        return handle;
    }

    private static string GetResourceVersionForApp(string appVersion)
    {
        // 예시: 간단한 매핑
        Debug.Log($"[CDNManager] GetResourceVersionForApp '{appVersion}'");
        switch (appVersion)
        {
            case "1.0.5": return "0.2.1";
            default:      return "0.2.2";
        }
    }
}
