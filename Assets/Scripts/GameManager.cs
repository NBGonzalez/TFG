// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string currentLanguage { get; private set; }
    public string currentLevelId { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SetCurrentLevel(string language, string levelId)
    {
        currentLanguage = language;
        currentLevelId = levelId;
    }
}
