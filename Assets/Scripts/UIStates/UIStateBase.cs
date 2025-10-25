using UnityEngine;

public abstract class UIStateBase : MonoBehaviour
{
    protected UIStateManager stateManager;
    
    public virtual void Init(UIStateManager manager)
    {

       stateManager = manager;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
}
