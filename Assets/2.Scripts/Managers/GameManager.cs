using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.AddressableAssets;
using UnityEngine.Audio;

[DefaultExecutionOrder(1)]
public class GameManager : MonoBehaviour
{
    // 타이틀 씬
    [Header("TitleButton")]
    [SerializeField] Button startButton;
    [SerializeField] Button soundButton;
    [SerializeField] Button exitButton;

    [Header("SoundPanel")]
    [SerializeField] GameObject soundPanel;
    [SerializeField] Button closeButton;

    [Header("Sound_Master")]
    [SerializeField] Slider masterSoundSlider;
    [SerializeField] TextMeshProUGUI masterText;

    [Header("Sound_BGM")]
    [SerializeField] Slider bgmSoundSlider;
    [SerializeField] TextMeshProUGUI bgmText;
    [SerializeField] AudioSource bgmAudioSource;

    [Header("Sound_SFX")]
    [SerializeField] Slider sfxSoundSlider;
    [SerializeField] TextMeshProUGUI sfxText;
    [SerializeField] AudioSource sfxAudioSource;

    [Header("SFX_Resource")]
    [SerializeField] AudioClip buttonPositiveConfirm;
    [SerializeField] AudioClip buttonNegativeCancel;

    // 메인 씬

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
        soundPanel.SetActive(false);

        // 로비
        startButton.onClick.AddListener(StartGame);
        soundButton.onClick.AddListener(OpenSoundPanel);
        exitButton.onClick.AddListener(ExitGame);

        // 사운드 패널
        closeButton.onClick.AddListener(CloseSoundPanel);

        // 사운드 볼륨
        masterSoundSlider.onValueChanged.AddListener(Master);
        bgmSoundSlider.onValueChanged.AddListener(BGM);
        sfxSoundSlider.onValueChanged.AddListener(SFX);
    }

    private void Update()
    {
        _managers.Update();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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

    void OpenSoundPanel()
    {
        sfxAudioSource.PlayOneShot(buttonPositiveConfirm);
        soundPanel.SetActive(true);
    }

    void ExitGame()
    {
        sfxAudioSource.PlayOneShot(buttonPositiveConfirm);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void CloseSoundPanel()
    {
        sfxAudioSource.PlayOneShot(buttonNegativeCancel);
        soundPanel.SetActive(false);
    }

    void Master(float value)
    {
        masterText.text = ((int)(value * 100)).ToString();
        AudioListener.volume = value;
    }

    void BGM(float value)
    {
        bgmText.text = ((int)(value * 100)).ToString();
        bgmAudioSource.volume = value;
    }

    void SFX(float value)
    {
        sfxText.text = ((int)(value * 100)).ToString();
        sfxAudioSource.volume = value;
    }
}