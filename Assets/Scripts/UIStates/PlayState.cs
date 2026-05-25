//PlayState.cs
// PlayState.cs
using UnityEngine;
using UnityEngine.UI;

public class PlayState : UIStateBase
{
    [SerializeField] private Button backButton;

    public override void OnEnter()
    {
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
    }
}