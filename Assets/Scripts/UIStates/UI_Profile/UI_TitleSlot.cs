using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TitleSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button myButton;
    [SerializeField] private GameObject lockIcon;

    // Variables internas
    private string myTitleId;
    private System.Action<string> onEquipCallback;
    private System.Action<ProfileTitleSO> onLockedCallback;
    private ProfileTitleSO myData;
    private bool isUnlocked;

    public void Setup(ProfileTitleSO data, bool unlocked,
                      System.Action<string> onClickEquip,
                      System.Action<ProfileTitleSO> onClickLocked = null)
    {
        myData = data;
        myTitleId = data.id;
        isUnlocked = unlocked;
        onEquipCallback = onClickEquip;
        onLockedCallback = onClickLocked;

        // 1. Poner datos visuales
        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.titleName;

        // 2. Gestionar estado Bloqueado/Desbloqueado
        if (isUnlocked)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            if (lockIcon != null) lockIcon.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 0.5f;
            canvasGroup.interactable = true; // Interactable para mostrar info del titulo
            if (lockIcon != null) lockIcon.SetActive(true);
        }

        // 3. Configurar el clic
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() =>
        {
            if (isUnlocked)
            {
                onEquipCallback?.Invoke(myTitleId);
            }
            else
            {
                onLockedCallback?.Invoke(myData);
            }
        });
    }
}