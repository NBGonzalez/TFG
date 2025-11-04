using UnityEngine;
using UnityEngine.UI;

public class PlayState : UIStateBase
{
    [SerializeField] private Button backButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnter()
    {
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));
        Debug.Log("STATE: Play");
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
    }
}
