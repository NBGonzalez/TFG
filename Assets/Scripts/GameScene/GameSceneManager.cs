using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Networking;


public class GameSceneManager : MonoBehaviour
{
    public Transform miniGameContainer; // Donde aparecerán los minijuegos (Canvas vacío)
    private LevelData currentLevelData;
    private int currentMiniGameIndex = 0;

    public GameObject explainPrefab;
    public GameObject quizzPrefab;
    public GameObject arrowsPrefab;
    public GameObject fillBlanksPrefab;


    private void Start()
    {
        StartCoroutine(LoadLevelAsync());

        if (currentLevelData != null)
            ShowMiniGame(currentMiniGameIndex);
        else
            Debug.LogError("No se pudo cargar el nivel antes de mostrar el minijuego.");
    }

    private IEnumerator LoadLevelAsync()
    {
        string language = GameManager.Instance.currentLanguage;
        string levelId = GameManager.Instance.currentLevelId;

        string fileName = $"{language.ToLower()}_{levelId.ToLower()}.json";
        string path = Path.Combine(Application.streamingAssetsPath, "levels", fileName);

        string json = "";

        // Igual que en UI_PlayScreen
        if (path.Contains("://") || path.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error al cargar {fileName}: {www.error}");
                    yield break;
                }
                else
                {
                    json = www.downloadHandler.text;
                }
            }
        }
        else
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"No se encontró el archivo del nivel: {path}");
                yield break;
            }
            json = File.ReadAllText(path);
        }

        currentLevelData = JsonConvert.DeserializeObject<LevelData>(json);

        if (currentLevelData == null)
        {
            Debug.LogError("Error al deserializar el JSON del nivel");
            yield break;
        }

        Debug.Log($"Nivel cargado: {currentLevelData.levelTitle} con {currentLevelData.minigames.Count} minijuegos.");

        // Ahora que está cargado, mostramos el primer minijuego
        ShowMiniGame(currentMiniGameIndex);
    }


    private void ShowMiniGame(int index)
    {
        if (currentLevelData == null || index >= currentLevelData.minigames.Count)
        {
            Debug.Log("Nivel completado");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }

        var data = currentLevelData.minigames[index];
        Debug.Log($"Mostrando minijuego: {data.type}");

        // Limpiar lo anterior
        foreach (Transform child in miniGameContainer)
            Destroy(child.gameObject);

        GameObject prefabToLoad = null;

        switch (data.type)
        {
            case "Explain":
                prefabToLoad = explainPrefab;
                break;
            case "Quizz":
                prefabToLoad = quizzPrefab;
                break;
            case "Arrows":
                prefabToLoad = arrowsPrefab;
                break;
            case "FillBlanks":
                prefabToLoad = fillBlanksPrefab;
                break;
        }

        if (prefabToLoad == null)
        {
            Debug.LogError($"No se encontró prefab para el tipo: {data.type}");
            return;
        }

        // Instanciar el prefab
        GameObject miniGameGO = Instantiate(prefabToLoad, miniGameContainer);

        switch (data.type)
        {
            case "Explain":
                miniGameGO.GetComponent<MiniGameExplain>().Show(data, this);
                break;

            case "Quizz":
                miniGameGO.GetComponent<MiniGameQuizz>().Show(data, this);
                break;

            case "Arrows":
                miniGameGO.GetComponent<MiniGameArrows>().Show(data, this);
                break;

            case "FillBlanks":
                //miniGameGO.GetComponent<MiniGameFillBlanks>().Show(data, this);
                break;

            default:
                Debug.LogError($"NO hay lógica para mostrar el minijuego: {data.type}");
                break;
        }

    }


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
