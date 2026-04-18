using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LocalItineraryCardUI : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;

    [Header("Botones")]
    public Button editButton;
    public Button deleteButton;

    // Variables internas para saber quiénes somos
    private string myItineraryId;
    private string myFilePath;

    private void Start()
    {
        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    // El Manager llamará a esta función para inyectar los datos del JSON
    public void SetupCard(string title, string author, string id, string filePath)
    {
        titleText.text = title;
        authorText.text = "Por: " + author;
        myItineraryId = id;
        myFilePath = filePath;
    }


    private void OnEditClicked()
    {
        Debug.Log("✏️ Editando: " + myItineraryId);

        // ¡USAMOS EL PUENTE! Dejamos la nota en el tablón estático
        ItineraryCrossSceneData.itineraryIdToEdit = myItineraryId;

        // Y cargamos la escena del creador (ajusta el nombre a tu sistema de transiciones)
        if (BackgroundTransition.Instance != null)
            BackgroundTransition.Instance.ToggleTransitionAndLoad("ItineraryScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("ItineraryScene");
    }

    private void OnDeleteClicked()
    {
        // 1. Borramos el archivo físico del disco duro
        if (File.Exists(myFilePath))
        {
            File.Delete(myFilePath);
            Debug.Log("🗑️ Archivo borrado: " + myFilePath);
        }

        // 2. Destruimos la placa visual
        Destroy(gameObject);
    }
}