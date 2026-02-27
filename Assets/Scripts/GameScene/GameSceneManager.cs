//GameSceneManager.cs
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Networking;

public class GameSceneManager : MonoBehaviour
{
    [Header("Mount points")]
    [Tooltip("Fallback: si no usas baseUIPrefab, se instanciarán los minijuegos aquí.")]
    public Transform miniGameContainer;

    [Header("Optional base UI (recommended)")]
    [Tooltip("Prefab que contiene Canvas, Title, Content, BackButton y GameArea (MiniGameBaseClass)")]
    public GameObject miniGameBaseClassPrefab;

    private MiniGameBaseClass miniGameBaseClass; // instancia única del UI base

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

    [Header("End Game UI")]
    public LevelCompletedUI levelCompletedInstance;

    [Header("Game Stats")]
    private int totalSuccess = 0;
    private int totalFailure = 0;

    private void Start()
    {
        //StartCoroutine(LoadLevelAsync());
        LoadLevelFromPlayFab();
    }
    private void LoadLevelFromPlayFab()
    {
        // Resetear estadísticas
        totalSuccess = 0;
        totalFailure = 0;

        // Leer selección guardada en GameManager
        string language = GameManager.Instance.currentLanguage;
        string levelId = GameManager.Instance.currentLevelId;

        // ¡LA MAGIA! Le pedimos el nivel a PlayFab y nos quedamos esperando a que responda
        PlayFabManager.Instancia.PedirNivel(language, levelId, OnNivelRecibido);
    }

    // Esta función se ejecuta automáticamente cuando PlayFab nos entrega el JSON
    private void OnNivelRecibido(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Error al cargar el nivel de PlayFab. Volviendo al menú.");
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
            Debug.Log("baseUIPrefab no asignado: se usará miniGameContainer como punto de montaje (fallback).");
        }

        // Mostrar el primer minijuego
        ShowMiniGame(currentMiniGameIndex);
    }

    //private IEnumerator LoadLevelAsync()
    //{
    //    // Resetear estadísticas
    //    totalSuccess = 0;
    //    totalFailure = 0;

    //    // Leer selección guardada en GameManager
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
    //            Debug.LogError($"No se encontró el archivo del nivel: {path}");
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
    //        Debug.Log("baseUIPrefab no asignado: se usará miniGameContainer como punto de montaje (fallback).");
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

        // Si hay baseUIInstance, actualizar título / content antes de instanciar contenido
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
            Debug.LogError($"No se encontró prefab para el tipo: {data.type}");
            return;
        }

        // Instanciar el prefab DENTRO del mountPoint (heredará el canvas del base UI si existe)
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
        // también buscar en hijos (en el caso de que el script esté en un child)
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

    public void RecordResult(bool esAcierto)
    {
        if (esAcierto) totalSuccess++;
        else totalFailure++;

        Debug.Log($"Score: Aciertos {totalSuccess} - Fallos {totalFailure}");
    }


    private void FinishLevel()
    {
        Debug.Log("Nivel completado. Calculando resultados...");

        // Ahora solo le pasamos los datos a la UI, y ella decidirá si guarda o no.

        // 1. Limpiar el último minijuego activo
        ClearCurrentMiniGame();

        // 2. Ocultar la UI Base del juego
        if (miniGameBaseClass != null)
        {
            miniGameBaseClass.gameObject.SetActive(false);
        }

        // 3. Mostrar pantalla de victoria con los NUEVOS DATOS
        if (levelCompletedInstance != null)
        {
            // --- CAMBIO: NUEVOS ARGUMENTOS ---
            // Ahora pasamos Language y LevelID para que la UI sepa qué nivel guardar luego
            levelCompletedInstance.Setup(
                currentLevelData.language,
                currentLevelData.levelId,
                currentLevelData.levelTitle,
                totalSuccess,
                totalFailure
            );
        }
        else
        {
            Debug.LogWarning("No has asignado LevelCompletedUI. Volviendo al menú.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }

    // -------------------------
    // API pública simple
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

