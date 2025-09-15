using UnityEngine;
using UnityEngine.UI;

public class SoundTest : MonoBehaviour
{
    public Button button;
    public AK.Wwise.Event sound;

    private void Start()
    {
        button.onClick.AddListener(() => PlaySound());
    }

    private void PlaySound()
    {
        sound.Post(gameObject);
    }
}
