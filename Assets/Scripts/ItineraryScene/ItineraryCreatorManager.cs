using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // IMPORTANTE para usar Listas

public class ItineraryCreatorManager : MonoBehaviour
{
    public static ItineraryCreatorManager Instance;

    [Header("Controles Globales UI")]
    public Button backButton;
    public Button finishButton;

    [Header("Creación de Niveles")]
    public Button createLevelButton;
    public ItineraryLevelCreatorPopup levelPopup;
    public Transform contentRoot;
    public GameObject prefabNivel;

    // ---- NUEVO: Creación de Minijuegos ----
    [Header("Creación de Minijuegos")]
    public MinigameCreatorPopup minigamePopup; // Arrastra tu Pop-up grande aquí
    public GameObject prefabMinijuego;         // Arrastra tu Prefab de la tarjetita pequeña aquí

    private ItineraryLevelUI nivelDestinoActual;


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

    private void OnFinishButtonPressed()
    {
        Debug.Log("💾 Finalizar pulsado.");
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
}