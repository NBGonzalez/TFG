using UnityEngine;
using UnityEngine.UI;

public class OptionsState : UIStateBase
{
    [SerializeField] private Button backButton;

    public override void OnEnter()
    {
        backButton.onClick.AddListener(OnBackPressed);
        Debug.Log("STATE: Options");
    }

    private void OnBackPressed()
    {
        stateManager.ChangeState("Main");
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
    }
}
