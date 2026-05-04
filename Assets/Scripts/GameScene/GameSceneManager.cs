//GameSceneManager.cs
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Networking;

public class GameSceneManager : MonoBehaviour
{
    [Header("Mount points")]
    [Tooltip("Fallback: si no usas baseUIPrefab, se instanciar�n los minijuegos aqu�.")]
    public Transform miniGameContainer;

    [Header("Optional base UI (recommended)")]
    [Tooltip("Prefab que contiene Canvas, Title, Content, BackButton y GameArea (MiniGameBaseClass)")]
    public GameObject miniGameBaseClassPrefab;

    private MiniGameBaseClass miniGameBaseClass; // instancia �nica del UI base

    private LevelData currentLevelData;
    private int currentMiniGameIndex = 0;

    [Header("Minigame prefabs")]
    public GameObject explainPrefab;
    public GameObject quizzPrefab;
    public GameObject arrowsPrefab;
    public GameObject fillBlanksPrefab;

    // Referencia al minijuego actualmente instanciado (si existe),
    // y su interfaz para poder TearDown() correctamente.
    private GameObject currentMiniGameGO;
    private IMiniGame currentIMiniGame;

    [Header("Failure UI")]
    public FailurePopupUI failurePopupInstance;

    [Header("Efectos Visuales")]
    public GlowController glowController;

    [Header("End Game UI")]
    public LevelCompletedUI levelCompletedInstance;

    [Header("Game Stats")]
    private int totalSuccess = 0;
    private int totalFailure = 0;

    private void Start()
    {
        if (GameManager.Instance.isLocal)
        {
            LoadLevelFromLocal();
        }
        else
        {
            LoadLevelFromPlayFab();
        }
    }
    private void LoadLevelFromPlayFab()
    {
        // Resetear estad�sticas
        totalSuccess = 0;
        totalFailure = 0;

        // Leer selecci�n guardada en GameManager
        string language = GameManager.Instance.currentLanguage;
        string levelId = GameManager.Instance.currentLevelId;

        // �LA MAGIA! Le pedimos el nivel a PlayFab y nos quedamos esperando a que responda
        PlayFabManager.Instancia.PedirNivel(language, levelId, OnNivelRecibido);
    }

    // Esta funci�n se ejecuta autom�ticamente cuando PlayFab nos entrega el JSON
    private void OnNivelRecibido(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Error al cargar el nivel de PlayFab. Volviendo al men�.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }

        // Deserializamos el texto igual que antes
        currentLevelData = JsonConvert.DeserializeObject<LevelData>(json);

        if (currentLevelData == null)
        {
            Debug.LogError("Error al deserializar el JSON del nivel");
            return;
        }

        Debug.Log($"Nivel listo en GameScene: {currentLevelData.levelTitle} con {currentLevelData.minigames.Count} minijuegos.");

        // Instanciar la UI base UNA sola vez (si se ha proporcionado)
        if (miniGameBaseClassPrefab != null)
        {
            GameObject go = Instantiate(miniGameBaseClassPrefab);
            miniGameBaseClass = go.GetComponent<MiniGameBaseClass>();
            if (miniGameBaseClass == null)
            {
                Debug.LogError("baseUIPrefab no contiene MiniGameBaseClass. Asigna el script en el prefab.");
            }
        }
        else
        {
            Debug.Log("baseUIPrefab no asignado: se usar� miniGameContainer como punto de montaje (fallback).");
        }

        // Mostrar el primer minijuego
        ShowMiniGame(currentMiniGameIndex);
    }

    private void LoadLevelFromLocal()
    {
        totalSuccess = 0;
        totalFailure = 0;

        // Si es local, "currentLanguage" guarda el ID del itinerario (ej: "custom-12345")
        string itineraryId = GameManager.Instance.currentLanguage;
        string targetLevelId = GameManager.Instance.currentLevelId;

        string path = Path.Combine(Application.persistentDataPath, itineraryId + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError("Archivo local no encontrado: " + path);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }

        // 1. Leemos el Super-JSON
        string json = File.ReadAllText(path);

        // Cuidado: Usamos JsonUtility porque as� guardamos en ItineraryCreatorManager
        CustomItineraryData customData = JsonUtility.FromJson<CustomItineraryData>(json);

        // 2. Buscamos el nivel espec�fico dentro de todo el itinerario
        CustomLevelData targetCustomLevel = customData.levels.Find(l => l.levelId == targetLevelId);

        if (targetCustomLevel == null)
        {
            Debug.LogError("Nivel no encontrado en el itinerario local.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }

        // 3. EL TRADUCTOR: Convertimos el CustomLevelData al LevelData cl�sico que esperan tus minijuegos
        currentLevelData = new LevelData();
        currentLevelData.language = itineraryId;
        currentLevelData.levelId = targetCustomLevel.levelId;
        currentLevelData.levelTitle = targetCustomLevel.levelTitle;
        // �La lista de minijuegos es id�ntica en ambas clases, se pasa directamente!
        currentLevelData.minigames = targetCustomLevel.minigames;

        Debug.Log($"[LOCAL] Nivel listo: {currentLevelData.levelTitle} con {currentLevelData.minigames.Count} minijuegos.");
        ConfigurarUIyEmpezar();
    }

    // He extra�do esta parte que se repet�a en ambos cargadores
    private void ConfigurarUIyEmpezar()
    {
        if (miniGameBaseClassPrefab != null)
        {
            GameObject go = Instantiate(miniGameBaseClassPrefab);
            miniGameBaseClass = go.GetComponent<MiniGameBaseClass>();
        }

        // Mostrar el primer minijuego
        ShowMiniGame(currentMiniGameIndex);
    }

    //private IEnumerator LoadLevelAsync()
    //{
    //    // Resetear estad�sticas
    //    totalSuccess = 0;
    //    totalFailure = 0;

    //    // Leer selecci�n guardada en GameManager
    //    string language = GameManager.Instance.currentLanguage;
    //    string levelId = GameManager.Instance.currentLevelId;

    //    string fileName = $"{language.ToLower()}_{levelId.ToLower()}.json";
    //    string path = Path.Combine(Application.streamingAssetsPath, "levels", fileName);

    //    string json = "";

    //    // Soporte para Android/WebGL: UnityWebRequest
    //    if (path.Contains("://") || path.Contains(":///"))
    //    {
    //        using (UnityWebRequest www = UnityWebRequest.Get(path))
    //        {
    //            yield return www.SendWebRequest();

    //            if (www.result != UnityWebRequest.Result.Success)
    //            {
    //                Debug.LogError($"Error al cargar {fileName}: {www.error}");
    //                yield break;
    //            }
    //            json = www.downloadHandler.text;
    //        }
    //    }
    //    else
    //    {
    //        if (!File.Exists(path))
    //        {
    //            Debug.LogError($"No se encontr� el archivo del nivel: {path}");
    //            yield break;
    //        }
    //        json = File.ReadAllText(path);
    //    }

    //    currentLevelData = JsonConvert.DeserializeObject<LevelData>(json);

    //    if (currentLevelData == null)
    //    {
    //        Debug.LogError("Error al deserializar el JSON del nivel");
    //        yield break;
    //    }

    //    Debug.Log($"Nivel cargado: {currentLevelData.levelTitle} con {currentLevelData.minigames.Count} minijuegos.");

    //    // Instanciar la UI base UNA sola vez (si se ha proporcionado)
    //    if (miniGameBaseClassPrefab != null)
    //    {
    //        GameObject go = Instantiate(miniGameBaseClassPrefab);
    //        miniGameBaseClass = go.GetComponent<MiniGameBaseClass>();
    //        if (miniGameBaseClass == null)
    //        {
    //            Debug.LogError("baseUIPrefab no contiene MiniGameBaseClass. Asigna el script en el prefab.");
    //            // seguimos pero sin baseUIInstance
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("baseUIPrefab no asignado: se usar� miniGameContainer como punto de montaje (fallback).");
    //    }

    //    // Mostrar el primer minijuego
    //    ShowMiniGame(currentMiniGameIndex);
    //}

    private void ShowMiniGame(int index)
    {
        if (index >= currentLevelData.minigames.Count)
        {
            FinishLevel();
            return;
        }

        if (currentLevelData == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }

        var data = currentLevelData.minigames[index];
        Debug.Log($"Mostrando minijuego: {data.type}");

        // Determinar punto de montaje (game area)
        Transform mountPoint = null;
        if (miniGameBaseClass != null && miniGameBaseClass.gameArea != null)
            mountPoint = miniGameBaseClass.gameArea;
        else if (miniGameContainer != null)
            mountPoint = miniGameContainer;
        else
        {
            Debug.LogError("No hay punto de montaje: asigna baseUIPrefab o miniGameContainer.");
            return;
        }

        // Limpiar el minijuego anterior (TearDown + Destroy)
        ClearCurrentMiniGame();

        // Si hay baseUIInstance, actualizar t�tulo / content antes de instanciar contenido
        if (miniGameBaseClass != null)
            miniGameBaseClass.Show(data, this);

        GameObject prefabToLoad = data.type switch
        {
            "Explain" => explainPrefab,
            "Quizz" => quizzPrefab,
            "Arrows" => arrowsPrefab,
            "FillBlanks" => fillBlanksPrefab,
            _ => null
        };

        if (prefabToLoad == null)
        {
            Debug.LogError($"No se encontr� prefab para el tipo: {data.type}");
            return;
        }

        // Instanciar el prefab DENTRO del mountPoint (heredar� el canvas del base UI si existe)
        currentMiniGameGO = Instantiate(prefabToLoad, mountPoint);

        // Buscar en los componentes del gameObject el primero que implemente IMiniGame
        currentIMiniGame = FindIMiniGameIn(currentMiniGameGO);
        if (currentIMiniGame != null)
        {
            // Inicializar por interfaz (polimorfismo claro)
            try
            {
                currentIMiniGame.Initialize(data, miniGameBaseClass);
                Debug.Log($"Inicializado por IMiniGame.Initialize() en {currentIMiniGame.GetType().Name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error inicializando minijuego ({currentIMiniGame.GetType().Name}): {ex}");
                // si hay error, limpiamos referencia para evitar llamadas futuras
                currentIMiniGame = null;
            }
        }
        else
        {
            Debug.LogWarning("El prefab instanciado no implementa IMiniGame. Implementa IMiniGame.Initialize(...) para inicializarlo.");
        }
    }

    // Busca un componente MonoBehaviour que implemente IMiniGame y devuelve la interfaz (o null)
    private IMiniGame FindIMiniGameIn(GameObject go)
    {
        var comps = go.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c is IMiniGame im) return im;
        }
        // tambi�n buscar en hijos (en el caso de que el script est� en un child)
        var childComps = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var c in childComps)
        {
            if (c is IMiniGame im) return im;
        }
        return null;
    }

    // Limpia el minijuego actual (llama TearDown si existe, y destruye el GameObject)
    private void ClearCurrentMiniGame()
    {
        if (currentIMiniGame != null)
        {
            try
            {
                currentIMiniGame.TearDown();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Exception en TearDown del minijuego actual: {ex}");
            }
            currentIMiniGame = null;
        }

        if (currentMiniGameGO != null)
        {
            Destroy(currentMiniGameGO);
            currentMiniGameGO = null;
        }
    }

    public void HandleMiniGameFailure(MiniGameData data, string question, string userAns, string correctAns)
    {
        currentLevelData.minigames.Add(data);
        ClearCurrentMiniGame();
        
        if (failurePopupInstance != null)
        {
            failurePopupInstance.Setup(question, userAns, correctAns);
        }
        else
        {
            Debug.LogWarning("No hay FailurePopupUI asignado. Pasando al siguiente minijuego.");
            NextMiniGame();
        }
    }

    public void RecordResult(bool esAcierto)
    {
        if (esAcierto) totalSuccess++;
        else totalFailure++;

        Debug.Log($"Score: Aciertos {totalSuccess} - Fallos {totalFailure}");
    }


    private void FinishLevel()
    {
        Debug.Log("Nivel completado. Calculando resultados...");

        // 1. Limpiar el �ltimo minijuego activo
        ClearCurrentMiniGame();

        // 2. Ocultar la UI Base del juego
        if (miniGameBaseClass != null)
        {
            miniGameBaseClass.gameObject.SetActive(false);
        }

        // 3. Mostrar pantalla de victoria con los NUEVOS DATOS
        if (levelCompletedInstance != null)
        {
            //  �EL PARCHE DEFINITIVO! 
            // Usamos GameManager.Instance.currentLanguage para garantizar que la llave 
            // de guardado ("custom-XXX") sea exactamente la misma que us� el mapa al entrar.
            levelCompletedInstance.Setup(
                GameManager.Instance.currentLanguage, // �La Llave Maestra!
                currentLevelData.levelId,
                currentLevelData.levelTitle,
                totalSuccess,
                totalFailure
            );
        }
        else
        {
            Debug.LogWarning("No has asignado LevelCompletedUI. Volviendo al men�.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }

    // -------------------------
    // API p�blica simple
    // -------------------------
    public void NextMiniGame()
    {
        currentMiniGameIndex++;
        ShowMiniGame(currentMiniGameIndex);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}


