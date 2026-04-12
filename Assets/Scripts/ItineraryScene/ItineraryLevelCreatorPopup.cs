using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItineraryLevelCreatorPopup : MonoBehaviour
{
    [Header("Campos de Texto")]
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;

    [Header("Botones")]
    public Button saveButton;
    public Button cancelButton;

    private ItineraryLevelUI nivelEnEdicion = null;

    private void Start()
    {
        // Enganchamos los botones a sus funciones
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(ClosePopup);
    }

    public void OpenPopup()
    {
        // Vaciamos los campos para que al crear un nivel nuevo no salgan textos antiguos
        titleInput.text = "";
        descriptionInput.text = "";

        // Encendemos el panel (Asegúrate de poner este script en el padre que oscurece la pantalla)
        gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        // Apagamos el panel
        gameObject.SetActive(false);
    }

    // Modo 1: Crear Nuevo
    public void OpenPopupForCreate()
    {
        nivelEnEdicion = null; // Empezamos de cero
        titleInput.text = "";
        descriptionInput.text = "";
        gameObject.SetActive(true);
    }

    // Modo 2: Editar Existente
    public void OpenPopupForEdit(ItineraryLevelUI nivel)
    {
        nivelEnEdicion = nivel; // Apuntamos a quién estamos editando
        titleInput.text = nivel.tituloPuro; // ¡Rellenamos con la memoria de la tarjeta!
        descriptionInput.text = nivel.descripcionPura;
        gameObject.SetActive(true);
    }

    private void OnSaveClicked()
    {
        if (string.IsNullOrWhiteSpace(titleInput.text)) return;

        if (nivelEnEdicion == null)
        {
            // Si el post-it está vacío, creamos uno nuevo como siempre
            ItineraryCreatorManager.Instance.CrearNuevoNivelVisual(titleInput.text, descriptionInput.text);
        }
        else
        {
            // Si hay alguien en el post-it, solo le actualizamos los textos (sin crear otra tarjeta)
            nivelEnEdicion.ConfigurarNivel(titleInput.text, descriptionInput.text);
        }

        ClosePopup();
    }
}