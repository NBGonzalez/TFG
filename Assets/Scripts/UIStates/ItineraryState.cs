using UnityEngine;
using UnityEngine.UI;

public class ItineraryState : UIStateBase
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button createItineraryButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnter()
    {
        ItineraryCrossSceneData.itineraryIdToEdit = null;
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));
        createItineraryButton.onClick.AddListener(() => BackgroundTransition.Instance.ToggleTransitionAndLoad("ItineraryScene"));
        //Debug.Log("STATE: Itinerary");
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
        createItineraryButton.onClick.RemoveAllListeners();
    }


}
