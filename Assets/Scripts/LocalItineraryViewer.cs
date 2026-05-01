using UnityEngine;
using System.IO;
using TMPro; // Necesario para el texto
using System.Collections; // Necesario para las corrutinas

public class LocalItineraryViewer : MonoBehaviour
{
    [Header("Referencias UI")]
    public Transform contentContainer; // El Content de tu ScrollView
    public GameObject cardPrefab;      // Tu nuevo Prefab_ItineraryCard

    [Header("Referencias Pop-up de Importación")]
    public GameObject importSuccessPopup;
    public TextMeshProUGUI importSuccessText;

    // Usamos OnEnable para que cada vez que entres a esta pantalla (este Estado), 
    // se actualice la lista automáticamente por si acabas de crear uno nuevo.
    private void OnEnable()
    {
        RefreshItineraryList();
        DeepLinkManager.OnItineraryImported += HandleNewItineraryImported;
    }
    private void OnDisable()
    {
        // MUY IMPORTANTE: Nos desuscribimos al salir para evitar errores de memoria
        DeepLinkManager.OnItineraryImported -= HandleNewItineraryImported;
    }
    // Esta función se ejecuta automáticamente cuando el DeepLink termina de guardar
    private void HandleNewItineraryImported(string title, string author)
    {
        // 1. Recargamos la lista visual para que el nuevo nivel aparezca de golpe
        RefreshItineraryList();

        // 2. Mostramos el Pop-up
        if (importSuccessPopup != null && importSuccessText != null)
        {
            string nombre = string.IsNullOrEmpty(title) ? "Sin título" : title;
            string creador = string.IsNullOrEmpty(author) ? "Desconocido" : author;

            importSuccessText.text = $"¡Itinerario '{nombre}' importado!\nCreado por: {creador}";
            importSuccessPopup.SetActive(true);

            // Lo ocultamos a los 3 segundos
            StartCoroutine(HidePopupAfterDelay(3f));
        }
    }
    private IEnumerator HidePopupAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (importSuccessPopup != null)
            importSuccessPopup.SetActive(false);
    }

    public void CloseImportPopup()
    {
        if (importSuccessPopup != null)
            importSuccessPopup.SetActive(false);
    }

    public void RefreshItineraryList()
    {
        // 1. Limpiamos la lista visual por si había placas viejas
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Buscamos TODOS los archivos JSON en nuestra carpeta segura
        // Nota: Solo buscamos los que tengan el formato de nuestro Creador para no mezclar otros datos
        string[] archivosGuardados = Directory.GetFiles(Application.persistentDataPath, "custom-*.json");

        // 3. Recorremos cada archivo que hemos encontrado
        foreach (string rutaArchivo in archivosGuardados)
        {
            try
            {
                // Leemos el texto del archivo
                string json = File.ReadAllText(rutaArchivo);

                // Lo convertimos a nuestro objeto para poder leer el título y el autor
                CustomItineraryData data = JsonUtility.FromJson<CustomItineraryData>(json);

                if (data != null)
                {
                    // ¡Creamos la placa!
                    GameObject nuevaPlaca = Instantiate(cardPrefab, contentContainer);
                    LocalItineraryCardUI scriptPlaca = nuevaPlaca.GetComponent<LocalItineraryCardUI>();

                    if (scriptPlaca != null)
                    {
                        scriptPlaca.SetupCard(data.title, data.authorName, data.itineraryId, rutaArchivo);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al leer el archivo " + rutaArchivo + ": " + e.Message);
            }
        }
    }
}