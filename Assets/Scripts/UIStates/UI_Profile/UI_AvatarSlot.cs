using UnityEngine;
using UnityEngine.UI;

public class UI_AvatarSlot : MonoBehaviour
{
    [SerializeField] private Image avatarIcon;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject selectionBorder; // El borde verde
    [SerializeField] private Button myButton;

    private string myId;
    private System.Action<string> onClickCallback;

    public void Setup(ProfileAvatarSO data, bool isUnlocked, bool isSelected, System.Action<string> onClick)
    {
        myId = data.id;
        onClickCallback = onClick;
        avatarIcon.sprite = data.avatarImage;

        // Estado Bloqueado/Desbloqueado
        if (isUnlocked)
        {
            lockIcon.SetActive(false);
            avatarIcon.color = Color.white;
            myButton.interactable = true;
        }
        else
        {
            lockIcon.SetActive(true);
            avatarIcon.color = Color.gray; // Oscurecer
            myButton.interactable = false; // No se puede equipar si está bloqueado
        }

        // Estado "Seleccionado" (Borde verde)
        selectionBorder.SetActive(isSelected);

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickCallback?.Invoke(myId));
    }
}