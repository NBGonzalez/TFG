using UnityEngine;
using UnityEngine.UI;

public class MainState : UIStateBase
{
    [SerializeField] private Button profileButton;
    [SerializeField] private Button friendsButton;
    [SerializeField] private Button optionsButton;

    public override void OnEnter()
    {
        profileButton.onClick.AddListener(() => stateManager.ChangeState("Profile"));
        friendsButton.onClick.AddListener(() => stateManager.ChangeState("Friends"));
        optionsButton.onClick.AddListener(() => stateManager.ChangeState("Options"));
        Debug.Log("STATE: Main");
    }

    public override void OnExit()
    {
        profileButton.onClick.RemoveAllListeners();
        friendsButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }
}

