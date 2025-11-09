using System.IO;
using UnityEngine;
using Newtonsoft.Json;


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
        LoadLevel();
        if (currentLevelData != null)
            ShowMiniGame(currentMiniGameIndex);
        else
            Debug.LogError("No se pudo cargar el nivel antes de mostrar el minijuego.");
    }

    private void LoadLevel()
    {
        // Obtener los datos guardados del GameManager
        string language = GameManager.Instance.currentLanguage;
        string levelId = GameManager.Instance.currentLevelId;

        string path = Path.Combine(Application.streamingAssetsPath, "levels", $"{language.ToLower()}_{levelId.ToLower()}.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"No se encontró el archivo del nivel: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        currentLevelData = JsonConvert.DeserializeObject<LevelData>(json);
        if (currentLevelData == null)
        {
            Debug.LogError("Error al deserializar el JSON del nivel");
            return;
        }

        Debug.Log($"Nivel cargado: {currentLevelData.levelTitle} con {currentLevelData.minigames.Count} minijuegos.");
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
        var explain = miniGameGO.GetComponent<MiniGameExplain>();
        explain.Show(data, this);
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
