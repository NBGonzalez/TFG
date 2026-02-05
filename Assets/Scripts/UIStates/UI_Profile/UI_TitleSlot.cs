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
    [SerializeField] private GameObject lockIcon; // (Opcional) Si quieres poner un candadito

    // Variables internas
    private string myTitleId;
    private System.Action<string> onEquipCallback;

    // Esta función la llamará el "Jefe" (ProfileState) para configurarme
    public void Setup(ProfileTitleSO data, bool isUnlocked, System.Action<string> onClickAction)
    {
        myTitleId = data.id;
        onEquipCallback = onClickAction;

        // 1. Poner datos visuales
        if (iconImage != null) iconImage.sprite = data.icon;
        if (titleText != null) titleText.text = data.titleName;

        // 2. Gestionar estado Bloqueado/Desbloqueado
        if (isUnlocked)
        {
            canvasGroup.alpha = 1f;       // Totalmente visible
            canvasGroup.interactable = true; // Se puede clicar
            if (lockIcon != null) lockIcon.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 0.5f;     // Semitransparente (apagado)
            canvasGroup.interactable = false; // No se puede clicar
            if (lockIcon != null) lockIcon.SetActive(true);
        }

        // 3. Configurar el clic
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() =>
        {
            // Al hacer clic, avisamos al Jefe: "¡Eh! Quieren equipar ESTE id"
            Debug.Log("¡CLIC DETECTADO EN EL BOTÓN!");
            onEquipCallback?.Invoke(myTitleId);
        });
    }
}