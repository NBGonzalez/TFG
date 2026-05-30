using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItineraryLevelUI : MonoBehaviour
{
    [Header("Textos y Contenedores")]
    public TextMeshProUGUI headerText;
    public GameObject contenedorMinijuegos;

    [Header("Botones de Acción")]
    public Button headerButton;      // Botón principal para Editar el nivel
    public Button plusButton;        // Añadir minijuego
    public Button deleteButton;      // Papelera
    public Button moveUpButton;      // Flecha Arriba
    public Button moveDownButton;    // Flecha Abajo

    [Header("Delete Confirmation")]
    [Tooltip("Texto que aparecerá cuando borres un nivel")]
    public string deleteText = $"¿Estás seguro de que quieres borrar el nivel?";

    // NUEVO: Variables ocultas para recordar los datos puros
    public string tituloPuro;
    public string descripcionPura;

    void Start()
    {
        if (headerButton != null) headerButton.onClick.AddListener(EditarEsteNivel);
        if (plusButton != null) plusButton.onClick.AddListener(ClickBotonMas);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (moveUpButton != null) moveUpButton.onClick.AddListener(OnMoveUpClicked);
        if (moveDownButton != null) moveDownButton.onClick.AddListener(OnMoveDownClicked);

        //deleteText = $"¿Estás seguro de que quieres borrar el nivel {headerText.text}?";
}

    public void ConfigurarNivel(string titulo, string descripcion)
    {
        tituloPuro = titulo;
        descripcionPura = descripcion;
        headerText.text = "Nivel: " + titulo;
    }

    private void EditarEsteNivel()
    {
        // ¡Magia! Le pasamos nuestra tarjeta al Manager para que nos edite
        ItineraryCreatorManager.Instance.AbrirEdicionNivel(this);
    }

    private void ClickBotonMas()
    {
        // ¡EL TRUCO SENIOR! Le pasamos "this" (este mismo script entero) al Manager
        ItineraryCreatorManager.Instance.AbrirPopupMinijuegos(this);
    }

    private void OnDeleteClicked()
    {
        if (ConfirmationPopupManager.Instance != null)
        {
            // Pasamos el texto del Inspector y la función de abajo como "receta"
            ConfirmationPopupManager.Instance.RequestConfirmation(deleteText, ExecuteDelete);
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