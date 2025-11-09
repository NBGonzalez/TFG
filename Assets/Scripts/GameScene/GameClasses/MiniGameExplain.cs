using UnityEngine;
using TMPro;

public class MiniGameExplain : MonoBehaviour
{
    public static MiniGameExplain Instance;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    private GameSceneManager manager;

    private void Awake() => Instance = this;

    public void Show(MiniGameData data, GameSceneManager mgr)
    {
        gameObject.SetActive(true);
        manager = mgr;

        titleText.text = data.title;
        contentText.text = data.content;
    }

    public void OnContinueButton()
    {
        gameObject.SetActive(false);
        manager.NextMiniGame();
    }
}

