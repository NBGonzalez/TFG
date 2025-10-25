using UnityEngine;
using UnityEngine.UI;

public class ProfileState : UIStateBase
{
    [SerializeField] private Button backButton;

    public override void OnEnter()
    {
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));
        Debug.Log("STATE: Profile");
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
    }
}

