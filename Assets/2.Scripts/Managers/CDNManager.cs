using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CDNManager : IManagerBase
{
    public static string CurrentCatalogVersion { get; private set; }
    public static string CurrentCatalogUrl { get; private set; }

    private const string BaseUrl = "https://s3.ap-southeast-2.amazonaws.com/com.bravo.citybuilder";

    public async void Init()
    {
        await InitializeAsync();
    }

    public static async UniTask InitializeAsync()
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
        
        var handle = Addressables.LoadContentCatalogAsync(CurrentCatalogUrl);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[Addressables] Catalog load failed: {CurrentCatalogUrl}");
        }
        else
        {
            Debug.Log($"[Addressables] Catalog loaded from: {CurrentCatalogUrl}");
        }
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
            case "1.0.0": return "0.1.2";
            case "1.0.1": return "0.1.2";
            case "1.0.2": return "0.1.3";
            case "1.0.3": return "0.1.4";
            default:      return "0.1.5";
        }
    }
}
