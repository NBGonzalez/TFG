using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance;

    // Diccionario por lenguaje: qué niveles ha completado
    private Dictionary<string, HashSet<string>> completedLevels = new();

    private const string SAVE_KEY = "player_progress_v2";

    void Awake()
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

    public bool IsLevelCompleted(string language, string levelId)
    {
        return completedLevels.ContainsKey(language) && completedLevels[language].Contains(levelId);
    }

    public void MarkLevelComplete(string language, string levelId)
    {
        if (!completedLevels.ContainsKey(language))
            completedLevels[language] = new HashSet<string>();

        completedLevels[language].Add(levelId);
        Save();
    }

    public void ResetProgress()
    {
        completedLevels.Clear();
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(new ProgressWrapper(completedLevels));
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            var wrapper = JsonUtility.FromJson<ProgressWrapper>(json);
            completedLevels = wrapper.ToDictionary();
        }
    }

    [System.Serializable]
    private class ProgressWrapper
    {
        public List<string> languages;
        public List<string[]> completedIds;

        public ProgressWrapper(Dictionary<string, HashSet<string>> data)
        {
            languages = new List<string>();
            completedIds = new List<string[]>();

            foreach (var kvp in data)
            {
                languages.Add(kvp.Key);
                completedIds.Add(new List<string>(kvp.Value).ToArray());
            }
        }

        public Dictionary<string, HashSet<string>> ToDictionary()
        {
            var dict = new Dictionary<string, HashSet<string>>();
            for (int i = 0; i < languages.Count; i++)
                dict[languages[i]] = new HashSet<string>(completedIds[i]);
            return dict;
        }
    }
}

