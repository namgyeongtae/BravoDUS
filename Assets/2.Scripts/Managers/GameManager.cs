using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1)]
public class GameManager : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] Button startButton;
    [SerializeField] AudioClip buttonPositiveConfirm;

    [Header("QuitPanel")]
    [SerializeField] GameObject quitPanel;
    [SerializeField] Button quitButton;
    [SerializeField] Button cancleButton;

    AudioSource audioSource;

    public GameObject QuitPanel => quitPanel;

    public static GameManager Instance { get; private set; }

    private Managers _managers = new Managers();

    public Managers Managers => _managers;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

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
        audioSource.PlayOneShot(buttonPositiveConfirm);
    }

    void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void Cancle()
    {
        quitPanel.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject newParent = GameObject.Find("SceneUI");
        quitPanel.transform.SetParent(newParent.transform, false);
    }
}