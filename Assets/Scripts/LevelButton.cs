using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public GameObject lockOverlay;

    public void Setup(string id, string title, string desc, bool unlocked, System.Action onClick)
    {
        titleText.text = title;
        descText.text = desc;

        button.interactable = unlocked;
        lockOverlay.SetActive(!unlocked);

        button.onClick.RemoveAllListeners();
        if (unlocked)
            button.onClick.AddListener(() => onClick.Invoke());
    }
}

