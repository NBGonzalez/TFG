using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string currentLanguage;
    public string currentLevelId;

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
