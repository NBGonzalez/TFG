using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItineraryMiniGameContainerUI : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI miniGameTypeText;

    [Header("Botones de Acción")]
    public Button editButton;
    public Button deleteButton;
    public Button moveUpButton;
    public Button moveDownButton;

    [Header("Delete Confirmation")]
    [Tooltip("Texto que aparecerá cuando borres un minijuego")]
    public string deleteText = "¿Estás seguro de que quieres borrar el minijuego?";

    // ¡EL CEREBRO! Aquí se guarda ABSOLUTAMENTE TODO el JSON de esta pregunta
    public MiniGameData miData;

    private void Start()
    {
        if (editButton != null) editButton.onClick.AddListener(OnEditClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (moveUpButton != null) moveUpButton.onClick.AddListener(OnMoveUpClicked);
        if (moveDownButton != null) moveDownButton.onClick.AddListener(OnMoveDownClicked);
    }

    // Ahora recibimos el paquete de datos completo
    public void ConfigurarTarjeta(MiniGameData nuevosDatos)
    {
        miData = nuevosDatos;
        miniGameTypeText.text = miData.type + ": " + miData.title;
    }

    private void OnEditClicked()
    {
        ItineraryCreatorManager.Instance.AbrirEdicionMinijuego(this);
    }

    private void OnDeleteClicked() {
        if (ConfirmationPopupManager.Instance != null)
        {
            // Pasamos el texto del Inspector y la función de abajo como "receta"
            ConfirmationPopupManager.Instance.RequestConfirmation(deleteText, ExecuteDelete);
            Debug.Log($"[ItineraryMiniGameContainerUI] Solicitud de confirmación para borrar el minijuego.");
            Debug.Log($"[ItineraryMiniGameContainerUI] Texto mostrado: {deleteText}");
        }
        else
        {
            // Seguro por si acaso: si no hay popup, borra directo
            ExecuteDelete();
        }
    }

    private void ExecuteDelete()
    {
        Destroy(gameObject);
    }

    private void OnMoveUpClicked()
    {
        int pos = transform.GetSiblingIndex();
        if (pos > 0) transform.SetSiblingIndex(pos - 1);
    }

    private void OnMoveDownClicked()
    {
        int pos = transform.GetSiblingIndex();
        if (pos < transform.parent.childCount - 1) transform.SetSiblingIndex(pos + 1);
    }
}