using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    private PlayerProgress progress;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "player_progress.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =============================
    //  PUBLIC API
    // =============================

    public bool IsLevelCompleted(string language, string levelId)
    {
        if (progress.completedLevels.ContainsKey(language))
        {
            return progress.completedLevels[language].Contains(levelId);
        }
        return false;
    }

    public void CompleteLevel(string language, string levelId)
    {
        if (!progress.completedLevels.ContainsKey(language))
            progress.completedLevels[language] = new List<string>();

        if (!progress.completedLevels[language].Contains(levelId))
        {
            progress.completedLevels[language].Add(levelId);
            Save();
        }
    }

    public List<string> GetCompletedLevels(string language)
    {
        if (!progress.completedLevels.ContainsKey(language))
            return new List<string>();

        return new List<string>(progress.completedLevels[language]);
    }

    // =============================
    // SAVE / LOAD
    // =============================

    private void Save()
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Progress] Guardado en: {SavePath}");
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            progress = new PlayerProgress();
            Save(); // crea archivo inicial
            return;
        }

        string json = File.ReadAllText(SavePath);
        progress = JsonUtility.FromJson<PlayerProgress>(json);

        if (progress.completedLevels == null)
            progress.completedLevels = new Dictionary<string, List<string>>();

        Debug.Log("[Progress] Progreso cargado.");
    }
}
