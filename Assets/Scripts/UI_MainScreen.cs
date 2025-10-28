using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class UI_MainScreen : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown languageDropdown;
    public RectTransform contentRoot;  // Content del ScrollView
    public GameObject levelButtonPrefab;

    private PathModel currentPath;

    private void Start()
    {
        // Configurar dropdown con lenguajes disponibles
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "SQL", "C++", "Python" });

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        // Cargar el primero (por defecto SQL)
        LoadLanguage("SQL");
    }

    private void OnLanguageChanged(int index)
    {
        string selected = languageDropdown.options[index].text;
        LoadLanguage(selected);
    }

    private void LoadLanguage(string language)
    {
        string fileName = "";
        switch (language)
        {
            case "SQL": fileName = "sql_path.json"; break;
            case "C++": fileName = "cpp_path.json"; break;
            case "Python": fileName = "python_path.json"; break;
        }

        string path = Path.Combine(Application.streamingAssetsPath, "paths", fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"No se encontró el archivo {path}");
            return;
        }

        string json = File.ReadAllText(path);
        currentPath = JsonUtility.FromJson<PathModel>(json);

        PopulateLevels(language);
    }

    private void PopulateLevels(string language)
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        var progress = PlayerProgressManager.Instance;

        foreach (var lvl in currentPath.levels)
        {
            bool unlocked = string.IsNullOrEmpty(lvl.requiredLevel) || progress.IsLevelCompleted(language, lvl.requiredLevel);

            GameObject btnGO = Instantiate(levelButtonPrefab, contentRoot);
            var button = btnGO.GetComponent<LevelButton>();
            button.Setup(lvl.id, lvl.title, lvl.description, unlocked, () => OnLevelClicked(language, lvl.id, unlocked));
        }
    }

    private void OnLevelClicked(string language, string levelId, bool unlocked)
    {
        if (!unlocked)
        {
            Debug.Log($"Nivel {levelId} bloqueado para {language}");
            return;
        }

        Debug.Log($"Abriendo nivel {levelId} de {language}");

        // Guardar selección actual antes de cargar la escena
        GameManager.Instance.SetCurrentLevel(language, levelId);

        // Cargar escena de minijuegos
        UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGamesScene");
    }
}

