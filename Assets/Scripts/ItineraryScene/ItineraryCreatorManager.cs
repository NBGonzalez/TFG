// Carga o crea los itinerarios personalizados de los jugadores. Es el gran "Director de Orquesta" que conecta la creación de Niveles y Minijuegos, y luego empaqueta todo en un JSON gigante listo para guardar o subir a la nube.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // IMPORTANTE para usar Listas
using System.IO;
using TMPro;

public class ItineraryCreatorManager : MonoBehaviour
{
    public static ItineraryCreatorManager Instance;

    [Header("Controles Globales UI")]
    public Button backButton;
    public Button finishButton;

    [Header("Creación de Niveles")]
    public TMP_InputField itineraryTitleInput;
    public Button createLevelButton;
    public ItineraryLevelCreatorPopup levelPopup;
    public Transform contentRoot;
    public GameObject prefabNivel;

    // ---- NUEVO: Creación de Minijuegos ----
    [Header("Creación de Minijuegos")]
    public MinigameCreatorPopup minigamePopup; // Arrastra tu Pop-up grande aquí
    public GameObject prefabMinijuego;         // Arrastra tu Prefab de la tarjetita pequeña aquí

    private ItineraryLevelUI nivelDestinoActual;
    private string currentItineraryId = null;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonPressed);
        if (finishButton != null) finishButton.onClick.AddListener(OnFinishButtonPressed);

        if (createLevelButton != null && levelPopup != null)
        {
            createLevelButton.onClick.RemoveAllListeners();
            // ¡CORRECCIÓN! Ahora llamamos a OpenPopupForCreate
            createLevelButton.onClick.AddListener(() => levelPopup.OpenPopupForCreate());
        }


        // Leemos el tablón de anuncios al cargar la escena
        if (!string.IsNullOrEmpty(ItineraryCrossSceneData.itineraryIdToEdit))
        {
            Debug.Log("[ItineraryCreatorManager] ¡Modo Edición detectado! Cargando: " + ItineraryCrossSceneData.itineraryIdToEdit);
            CargarItinerarioParaEditar(ItineraryCrossSceneData.itineraryIdToEdit);

            // Borramos la nota del tablón por si el jugador sale y luego le da a "Crear Nuevo"
            ItineraryCrossSceneData.itineraryIdToEdit = null;
        }
        else
        {
            Debug.Log("[ItineraryCreatorManager] ¡Modo Creación detectado! Lienzo en blanco.");
            currentItineraryId = null;
        }
    }
    private void CargarItinerarioParaEditar(string id)
    {
        string nombreArchivo = id + ".json";
        string rutaCompleta = Path.Combine(Application.persistentDataPath, nombreArchivo);

        if (File.Exists(rutaCompleta))
        {
            // 1. Leemos el texto y lo convertimos al objeto
            string json = File.ReadAllText(rutaCompleta);
            CustomItineraryData data = JsonUtility.FromJson<CustomItineraryData>(json);

            // 2. Apuntamos el ID para sobrescribirlo luego al guardar
            currentItineraryId = data.itineraryId;

            // 3. Rellenamos el título principal
            if (itineraryTitleInput != null)
            {
                itineraryTitleInput.text = data.language; // O data.title, el que prefieras mostrar
            }

            // 4. Reconstruimos visualmente los Niveles
            foreach (CustomLevelData nivelData in data.levels)
            {
                // Instanciamos tarjeta azul
                GameObject nuevaTarjetaNivel = Instantiate(prefabNivel, contentRoot);
                ItineraryLevelUI scriptNivel = nuevaTarjetaNivel.GetComponent<ItineraryLevelUI>();

                if (scriptNivel != null)
                {
                    scriptNivel.ConfigurarNivel(nivelData.levelTitle, nivelData.levelDescription);

                    // 5. Reconstruimos visualmente los Minijuegos DENTRO del Nivel
                    foreach (MiniGameData minigameData in nivelData.minigames)
                    {
                        GameObject nuevaTarjetaMini = Instantiate(prefabMinijuego, scriptNivel.contenedorMinijuegos.transform);
                        ItineraryMiniGameContainerUI scriptMinijuego = nuevaTarjetaMini.GetComponent<ItineraryMiniGameContainerUI>();

                        if (scriptMinijuego != null)
                        {
                            // Le inyectamos la memoria completa que traía el JSON
                            scriptMinijuego.ConfigurarTarjeta(minigameData);
                        }
                    }
                }
            }

            Canvas.ForceUpdateCanvases();
            Debug.Log("[ItineraryCreatorManager] Itinerario reconstruido con éxito.");
        }
        else
        {
            Debug.LogError("[ItineraryCreatorManager] No se ha encontrado el archivo para editar: " + rutaCompleta);
        }
    }

    public void CrearNuevoNivelVisual(string titulo, string descripcion)
    {
        GameObject nuevaTarjeta = Instantiate(prefabNivel, contentRoot);
        ItineraryLevelUI scriptNivel = nuevaTarjeta.GetComponent<ItineraryLevelUI>();

        if (scriptNivel != null)
        {
            // ¡CORRECCIÓN! Ahora le pasamos ambas cosas a la tarjeta
            scriptNivel.ConfigurarNivel(titulo, descripcion);
        }

        Canvas.ForceUpdateCanvases();
    }

    // ==========================================
    // NUEVAS FUNCIONES PARA LOS MINIJUEGOS
    // ==========================================

    // Fíjate cómo ahora recibe un "ItineraryLevelUI" entero, no un simple número
    public void AbrirPopupMinijuegos(ItineraryLevelUI nivelQueLoPide)
    {
        // Guardamos el nivel físico en nuestro post-it mental
        nivelDestinoActual = nivelQueLoPide;

        // ¡CORRECCIÓN! Ahora llamamos a OpenPopupForCreate
        minigamePopup.OpenPopupForCreate();
    }

    // ¡Cambio aquí! Recibe el objeto entero MiniGameData
    public void CrearNuevoMinijuegoVisual(MiniGameData dataCompleta)
    {
        GameObject nuevaTarjeta = Instantiate(prefabMinijuego, nivelDestinoActual.contenedorMinijuegos.transform);

        ItineraryMiniGameContainerUI scriptMinijuego = nuevaTarjeta.GetComponent<ItineraryMiniGameContainerUI>();
        if (scriptMinijuego != null)
        {
            // Le pasamos el paquete entero a la tarjeta
            scriptMinijuego.ConfigurarTarjeta(dataCompleta);
        }

        Canvas.ForceUpdateCanvases();
    }

    // ... (Aquí siguen tus funciones OnBackButtonPressed y OnFinishButtonPressed de antes) ...
    private void OnBackButtonPressed()
    {
        if (BackgroundTransition.Instance != null) BackgroundTransition.Instance.ToggleTransitionAndLoad("MainScene");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // Abre el pop-up de Nivel, pero en modo Edición
    public void AbrirEdicionNivel(ItineraryLevelUI nivelAEditar)
    {
        levelPopup.OpenPopupForEdit(nivelAEditar);
    }

    // Abre el pop-up de Minijuegos, pero en modo Edición
    public void AbrirEdicionMinijuego(ItineraryMiniGameContainerUI minijuegoAEditar)
    {
        minigamePopup.OpenPopupForEdit(minijuegoAEditar);
    }

    private void OnFinishButtonPressed()
    {
        Debug.Log("[ItineraryCreatorManager] ¡Construyendo el Mega-JSON Final...");

        string tituloFinal = "Itinerario Personalizado";
        if (itineraryTitleInput != null && !string.IsNullOrWhiteSpace(itineraryTitleInput.text))
        {
            tituloFinal = itineraryTitleInput.text;
        }

        // 1. Creamos el contenedor principal vacío
        CustomItineraryData itinerarioFinal = new CustomItineraryData();
        itinerarioFinal.language = tituloFinal;
        itinerarioFinal.title = tituloFinal;
        //itinerarioFinal.itineraryId = "custom-" + System.Guid.NewGuid().ToString();         // Genera un ID matemáticamente único en el universo
        itinerarioFinal.levels = new List<CustomLevelData>();
        itinerarioFinal.authorName = LoginManager.Instance.GetPlayerName(); // ¡Recuperamos el nombre del autor desde el LoginManager!
        Debug.Log("[ItineraryCreatorManager] Nombre del autor recuperado: " + itinerarioFinal.authorName);

        // Si ya teníamos un ID porque estamos editando, lo mantenemos. Si no, creamos uno nuevo.
        if (string.IsNullOrEmpty(currentItineraryId))
        {
            currentItineraryId = "custom-" + System.Guid.NewGuid().ToString();
        }
        itinerarioFinal.itineraryId = currentItineraryId;

        // 2. Recorremos TODOS los niveles que hay colgados en nuestro Content (El ScrollView)
        foreach (Transform hijoNivel in contentRoot)
        {
            ItineraryLevelUI scriptNivel = hijoNivel.GetComponent<ItineraryLevelUI>();

            // Si realmente es un nivel...
            if (scriptNivel != null)
            {
                CustomLevelData nivelData = new CustomLevelData();
                nivelData.levelId = "level-" + System.Guid.NewGuid().ToString().Substring(0, 5); // Un ID cortito
                nivelData.levelTitle = scriptNivel.tituloPuro;
                nivelData.levelDescription = scriptNivel.descripcionPura;
                nivelData.minigames = new List<MiniGameData>();

                // 3. Recorremos TODOS los minijuegos que hay dentro del contenedor de este nivel
                foreach (Transform hijoMinijuego in scriptNivel.contenedorMinijuegos.transform)
                {
                    ItineraryMiniGameContainerUI scriptMinijuego = hijoMinijuego.GetComponent<ItineraryMiniGameContainerUI>();

                    if (scriptMinijuego != null)
                    {
                        // ¡Añadimos el paquete de memoria intacto a la lista!
                        nivelData.minigames.Add(scriptMinijuego.miData);
                    }
                }

                // Añadimos el nivel completo al itinerario
                itinerarioFinal.levels.Add(nivelData);
            }
        }

        // 4. Transformamos todo ese objeto masivo en un bonito texto JSON
        // El "true" final es para que lo formatee con saltos de línea (Pretty Print) y sea legible
        string jsonFinal = JsonUtility.ToJson(itinerarioFinal, true);

        // Definimos la ruta: Se guardará en una carpeta especial de tu PC
        string nombreArchivo = itinerarioFinal.itineraryId + ".json";
        string rutaCompleta = Path.Combine(Application.persistentDataPath, nombreArchivo);
        Debug.Log("[ItineraryCreatorManager] Ruta completa donde se guardará el JSON:\n" + rutaCompleta);

        // Escribimos el archivo en el disco duro
        File.WriteAllText(rutaCompleta, jsonFinal);

        // Lo imprimimos en la consola para celebrar
        Debug.Log("<color=green><b>¡MEGA-JSON CREADO CON ÉXITO!</b></color>\n" + jsonFinal);

        // El siguiente paso lógico iría aquí (Subirlo a PlayFab o guardarlo en el PC)
    }
}