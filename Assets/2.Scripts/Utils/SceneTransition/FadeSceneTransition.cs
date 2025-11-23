using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FadeSceneTransition : SceneTransition
{
    private Image _fadeImage;

    protected override void Initialize()
    {
        base.Initialize();

        _fadeImage = GetComponent<Image>();
    }

    public override async UniTask AnimateTransitionIn()
    {
        var image = GetComponent<Image>();

        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.0f);
        
        await UniTask.Delay(200);

        while (image.color.a < 1.0f)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + Time.deltaTime);
            await UniTask.DelayFrame(1);
        }

        // CanvasManager.Instance.RemovePanel(this);
    }

    public override async UniTask AnimateTransitionOut()
    {
        var image = GetComponent<Image>();

        image.color = new Color(image.color.r, image.color.g, image.color.b, 1.0f);
        
        await UniTask.Delay(200);


        while (image.color.a > 0.0f)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - Time.deltaTime);
            await UniTask.DelayFrame(1);
        }

        // CanvasManager.Instance.RemovePanel(this);
    }
}
