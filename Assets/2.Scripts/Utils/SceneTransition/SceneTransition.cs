using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SceneTransition : CanvasPanel
{
    public virtual async UniTask AnimateTransitionIn()
    {
        
    }
    public virtual async UniTask AnimateTransitionOut()
    {

    }
}
