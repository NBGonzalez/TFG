using UnityEngine;
using UnityEngine.UI;

public class MainState : UIStateBase
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button friendsButton;
    [SerializeField] private Button optionsButton;

    public override void OnEnter()
    {
        playButton.onClick.AddListener(() => stateManager.ChangeState("Play"));
        profileButton.onClick.AddListener(() => stateManager.ChangeState("Profile"));
        friendsButton.onClick.AddListener(() => stateManager.ChangeState("Friends"));
        optionsButton.onClick.AddListener(() => stateManager.ChangeState("Options"));
    }

    public override void OnExit()
    {
        playButton.onClick.RemoveAllListeners();
        profileButton.onClick.RemoveAllListeners();
        friendsButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }
}


