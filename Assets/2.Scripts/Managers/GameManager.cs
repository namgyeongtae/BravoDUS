using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1)]
public class GameManager : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] Button startButton; 

    [Header("QuitPanel")]
    [SerializeField] GameObject quitPanel;
    [SerializeField] Button quitButton;
    [SerializeField] Button cancleButton;

    [Header("AudioSource")]
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource sfxAudioSource;

    [Header("AudioClip")]
    [SerializeField] AudioClip buttonPositiveConfirm;
    [SerializeField] AudioClip buttonNegativeCancle;


    public GameObject QuitPanel => quitPanel;
    public AudioSource BgmAudioSource => bgmAudioSource;
    public AudioSource SfxAudioSource => sfxAudioSource;

    public static GameManager Instance { get; private set; }

    private Managers _managers = new Managers();

    public Managers Managers => _managers;

    private void Awake()
    {
        Instance = this;

        _managers.Init();

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        quitPanel.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(Quit);
        cancleButton.onClick.AddListener(Cancle);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        _managers.Update();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitPanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        _managers.Release();
    }

    void StartGame()
    {
        sfxAudioSource.PlayOneShot(buttonPositiveConfirm);
    }

    void Quit()
    {
        sfxAudioSource.PlayOneShot(buttonPositiveConfirm);

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void Cancle()
    {
        sfxAudioSource.PlayOneShot(buttonNegativeCancle);
        quitPanel.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject newParent = GameObject.Find("SceneUI");
        quitPanel.transform.SetParent(newParent.transform, false);
    }
}