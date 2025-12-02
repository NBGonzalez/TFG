using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameExplain : MonoBehaviour, IMiniGame
{

    [SerializeField] private Button continueButton; // opcional en el prefab

    private MiniGameData data;
    private MiniGameBaseClass baseUI;

    public void Initialize(MiniGameData data, MiniGameBaseClass baseUI)
    {
        this.data = data;
        this.baseUI = baseUI;


        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                // usar coroutine del propio MonoBehaviour para respetar el delay de la base
                StartCoroutine(baseUI.NextMiniGameDelayed(0.15f));
            });
        }
    }

    public void TearDown()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();
    }
}
