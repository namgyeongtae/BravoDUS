using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBindBase : MonoBehaviour
{
   private bool _isBindingDone = false;
   protected bool IsBindingDone => _isBindingDone;

   protected virtual void Awake()
   {
        if (!_isBindingDone)
            InstallBindings();
   }

   public void InstallBindings()
   {
        if (_isBindingDone)
        {
            Debug.Log($"Bind already done. {this.gameObject.name}");
            return;
        }
        Debug.Log($"Start Bind : {this.gameObject.name}");
        BindAttribute.InstallBindings(this);
        _isBindingDone = true;
   }

   protected virtual void Initialize() { }

   protected static void BindEvent(Button InComponent, UnityAction InAction)
   {
        InComponent?.onClick.AddListener(InAction);
   }
}
