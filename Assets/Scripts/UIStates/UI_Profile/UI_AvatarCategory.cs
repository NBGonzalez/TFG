// UI_AvatarCategory.cs
using UnityEngine;
using TMPro;

public class UI_AvatarCategory : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryTitle;
    [SerializeField] private Transform gridContent;

    public Transform GridContent => gridContent;

    public void Setup(string title, int unlockedInCategory, int totalInCategory)
    {
        if (categoryTitle != null)
            categoryTitle.text = $"{title} ({unlockedInCategory}/{totalInCategory})";
    }
}