using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance { get; private set; }

    // Sound Option Panel
    [Header("Master")]
    [SerializeField] Slider masterSoundSlider;
    [SerializeField] TextMeshProUGUI masterText;

    [Header("BGM")]
    [SerializeField] Slider bgmSoundSlider;
    [SerializeField] TextMeshProUGUI bgmText;

    [Header("SFX")]
    [SerializeField] Slider sfxSoundSlider;
    [SerializeField] TextMeshProUGUI sfxText;
    [SerializeField] AudioSource sfxAudioSource; 

    [Header("Panel_Button")]
    [SerializeField] GameObject soundPanel;
    [SerializeField] Button closeButton;

    // Resource
    [Header("SFX_Resource")]
    [SerializeField] AudioClip buttonPositiveConfirm;
    [SerializeField] AudioClip buttonNegativeCancel;

    // SFX
    [Header("Button")]
    [SerializeField] Button optionButton;

    public AudioSource SfxAudioSource => sfxAudioSource;
    public AudioClip ButtonPositiveConfirm => buttonPositiveConfirm;
    public AudioClip ButtonNegativeCancel => buttonNegativeCancel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        soundPanel.SetActive(false);

        // 옵션 버튼
        optionButton.onClick.AddListener(OpenSoundPanel);

        // 사운드 패널
        closeButton.onClick.AddListener(CloseSoundPanel);

        // 사운드 볼륨
        masterSoundSlider.onValueChanged.AddListener(Master);
        bgmSoundSlider.onValueChanged.AddListener(BGM);
        sfxSoundSlider.onValueChanged.AddListener(SFX);
    }

    void OpenSoundPanel()
    {
        PlayButtonPositive();
        soundPanel.SetActive(true);
    }

    void CloseSoundPanel()
    {
        PlayButtonNegative();
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
        GameManager.Instance.BgmAudioSource.volume = value;
    }

    void SFX(float value)
    {
        sfxText.text = ((int)(value * 100)).ToString();
        GameManager.Instance.SfxAudioSource.volume = value;
        sfxAudioSource.volume = value;
    }

    public void PlayButtonPositive()
    {
        sfxAudioSource.PlayOneShot(buttonPositiveConfirm);
    }

    public void PlayButtonNegative()
    {
        sfxAudioSource.PlayOneShot(buttonNegativeCancel);
    }
}