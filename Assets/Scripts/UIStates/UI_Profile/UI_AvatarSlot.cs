using UnityEngine;
using UnityEngine.UI;

public class UI_AvatarSlot : MonoBehaviour
{
    [SerializeField] private Image avatarIcon;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject selectionBorder;
    [SerializeField] private Button myButton;

    private string myId;
    private System.Action<string> onClickCallback;
    private System.Action<ProfileAvatarSO> onLockedCallback;
    private ProfileAvatarSO myData;
    private bool isUnlocked;

    public void Setup(ProfileAvatarSO data, bool unlocked, bool isSelected,
                      System.Action<string> onClick,
                      System.Action<ProfileAvatarSO> onClickLocked = null)
    {
        myData = data;
        myId = data.id;
        isUnlocked = unlocked;
        onClickCallback = onClick;
        onLockedCallback = onClickLocked;

        avatarIcon.sprite = data.avatarImage;

        // Estado Bloqueado/Desbloqueado
        if (isUnlocked)
        {
            lockIcon.SetActive(false);
            avatarIcon.color = Color.white;
        }
        else
        {
            lockIcon.SetActive(true);
            avatarIcon.color = Color.gray;
        }

        // Siempre interactable para poder mostrar info del bloqueado
        myButton.interactable = true;

        // Estado "Seleccionado" (Borde verde)
        selectionBorder.SetActive(isSelected);

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() =>
        {
            if (isUnlocked)
            {
                onClickCallback?.Invoke(myId);
            }
            else
            {
                onLockedCallback?.Invoke(myData);
            }
        });
    }
}