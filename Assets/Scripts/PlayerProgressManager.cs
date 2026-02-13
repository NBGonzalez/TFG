// PlayerProgressManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System.Linq; // Necesario para .Keys.ToList()

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    private PlayerProgress progress;

    private const string CLOUD_KEY = "player_progress";
    private string SavePath => Path.Combine(Application.persistentDataPath, "player_progress.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocal();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AuthenticationService.Instance.SignedIn += OnUserSignedIn;
        if (AuthenticationService.Instance.IsSignedIn)
        {
            OnUserSignedIn();
        }
    }

    private void OnDestroy()
    {
        try
        {
            if (AuthenticationService.Instance != null)
                AuthenticationService.Instance.SignedIn -= OnUserSignedIn;
        }
        catch { }
    }

    private async void OnUserSignedIn()
    {
        Debug.Log("[Progress] Usuario detectado. Sincronizando nube...");
        await LoadFromCloudAsync();
        CheckDailyStreak();
    }

    // =============================
    //  API PÚBLICA OPTIMIZADA
    // =============================

    public bool IsLevelCompleted(string language, string levelId)
    {
        // AHORA MIRAMOS LAS ESTRELLAS
        // Si tienes más de 0 estrellas, el nivel está completado.
        return GetStarsForLevel(language, levelId) > 0;
    }

    public int GetStarsForLevel(string language, string levelId)
    {
        if (progress.levelStars != null &&
            progress.levelStars.ContainsKey(language) &&
            progress.levelStars[language].ContainsKey(levelId))
        {
            return progress.levelStars[language][levelId];
        }
        return 0;
    }

    public async void CompleteLevel(string language, string levelId, int starsEarned)
    {
        // REGLA DE ORO: Si sacas 0 estrellas (suspenso), no guardamos nada.
        if (starsEarned <= 0) return;

        // Inicializar diccionarios si no existen
        if (!progress.levelStars.ContainsKey(language))
            progress.levelStars[language] = new Dictionary<string, int>();

        // Obtener puntuación anterior
        int previousStars = 0;
        if (progress.levelStars[language].ContainsKey(levelId))
        {
            previousStars = progress.levelStars[language][levelId];
        }

        // SOLO GUARDAMOS SI MEJORAMOS (O IGUALAMOS) LA PUNTUACIÓN
        // (Si sacaste 3 estrellas y ahora sacas 1, nos quedamos con el 3)
        if (starsEarned > previousStars)
        {
            progress.levelStars[language][levelId] = starsEarned;

            // Sumamos la diferencia al total de perfil
            int difference = starsEarned - previousStars;
            AddStars(difference);

            Debug.Log($"[Progress] Nivel {levelId} completado. Nuevo récord: {starsEarned} estrellas.");

            // Guardamos inmediatamente
            SaveLocal();
            await SaveToCloudAsync();
        }
        else
        {
            Debug.Log($"[Progress] Nivel {levelId} completado, pero no se superó el récord anterior ({previousStars}).");
        }
    }

    // Helper por si alguna UI necesita la lista de IDs
    public List<string> GetCompletedLevels(string language)
    {
        if (progress.levelStars.ContainsKey(language))
        {
            // Devolvemos las claves (IDs) del diccionario de estrellas
            return progress.levelStars[language].Keys.ToList();
        }
        return new List<string>();
    }

    // =============================
    //  RESTO DE FUNCIONALIDADES
    // =============================

    public void CheckDailyStreak()
    {
        if (string.IsNullOrEmpty(progress.lastLoginDate))
        {
            progress.currentStreak = 1;
            UpdateLastLoginDate();
            SaveLocal();
            return;
        }

        if (!System.DateTime.TryParse(progress.lastLoginDate, out System.DateTime lastDate))
        {
            UpdateLastLoginDate();
            return;
        }

        double daysDiff = (System.DateTime.Now.Date - lastDate).TotalDays;

        if (daysDiff < 1) return; // Mismo día

        if (daysDiff >= 1 && daysDiff < 2)
        {
            progress.currentStreak++;
            Debug.Log($"[Streak] Racha: {progress.currentStreak}");
        }
        else
        {
            progress.currentStreak = 1;
            Debug.Log("[Streak] Racha perdida.");
        }

        UpdateLastLoginDate();
        SaveLocal();
    }

    private void UpdateLastLoginDate()
    {
        progress.lastLoginDate = System.DateTime.Now.Date.ToString();
    }

    public void AddStars(int amount)
    {
        progress.totalStars += amount;
        SaveLocal();
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!progress.unlockedAchievements.Contains(achievementId))
        {
            progress.unlockedAchievements.Add(achievementId);
            SaveLocal();
        }
    }

    public void EquipTitle(string titleId)
    {
        if (titleId == "Novato" || progress.unlockedAchievements.Contains(titleId))
        {
            progress.equippedTitleId = titleId;
            SaveLocal();
        }
    }

    public bool HasUnlocked(string achievementId)
    {
        if (progress == null || progress.unlockedAchievements == null) return false;
        return progress.unlockedAchievements.Contains(achievementId);
    }

    public void EquipAvatar(string avatarId)
    {
        progress.currentAvatarId = avatarId;
        SaveLocal();
        _ = SaveToCloudAsync();
    }

    public string GetEquippedAvatarId()
    {
        if (string.IsNullOrEmpty(progress.currentAvatarId)) return "avatar_default";
        return progress.currentAvatarId;
    }

    public int GetTotalStars() => progress.totalStars;
    public int GetStreak() => progress.currentStreak;
    public string GetEquippedTitle() => progress.equippedTitleId;

    // =============================
    //  SAVE / LOAD / CLOUD
    // =============================

    private async Task SaveToCloudAsync()
    {
        try
        {
            var dataToSave = new Dictionary<string, object> { { CLOUD_KEY, progress } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
        }
        catch (System.Exception e) { Debug.LogError($"[Cloud Save Error] {e.Message}"); }
    }

    private async Task LoadFromCloudAsync()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { CLOUD_KEY });

            if (savedData.TryGetValue(CLOUD_KEY, out var cloudItem))
            {
                PlayerProgress cloudProgress = cloudItem.Value.GetAs<PlayerProgress>();

                if (progress == null)
                {
                    progress = cloudProgress;
                }
                else
                {
                    // --- FUSIÓN OPTIMIZADA (Solo Estrellas) ---
                    if (cloudProgress.levelStars != null)
                    {
                        foreach (var langEntry in cloudProgress.levelStars)
                        {
                            string language = langEntry.Key;
                            if (!progress.levelStars.ContainsKey(language))
                                progress.levelStars[language] = new Dictionary<string, int>();

                            foreach (var levelEntry in langEntry.Value)
                            {
                                string lvlId = levelEntry.Key;
                                int cloudStars = levelEntry.Value;
                                int localStars = 0;

                                if (progress.levelStars[language].ContainsKey(lvlId))
                                    localStars = progress.levelStars[language][lvlId];

                                // Nos quedamos con la mejor puntuación
                                if (cloudStars > localStars)
                                {
                                    progress.levelStars[language][lvlId] = cloudStars;
                                }
                            }
                        }
                    }

                    // Fusión de Stats (Mayor valor gana)
                    if (cloudProgress.totalStars > progress.totalStars) progress.totalStars = cloudProgress.totalStars;
                    if (cloudProgress.currentStreak > progress.currentStreak) progress.currentStreak = cloudProgress.currentStreak;

                    // Fusión de Logros
                    foreach (var ach in cloudProgress.unlockedAchievements)
                    {
                        if (!progress.unlockedAchievements.Contains(ach))
                            progress.unlockedAchievements.Add(ach);
                    }
                }
                SaveLocal();
                await SaveToCloudAsync();
            }
            else if (progress != null && progress.totalStars > 0)
            {
                await SaveToCloudAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cloud Load Error] {e.Message}");
            if (progress == null) LoadLocal();
        }
    }

    private void SaveLocal()
    {
        string json = JsonConvert.SerializeObject(progress, Formatting.Indented);
        File.WriteAllText(SavePath, json);
    }

    private void LoadLocal()
    {
        if (!File.Exists(SavePath))
        {
            progress = new PlayerProgress();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            progress = JsonConvert.DeserializeObject<PlayerProgress>(json) ?? new PlayerProgress();

            // Null Checks (Vitales)
            if (progress.levelStars == null) progress.levelStars = new Dictionary<string, Dictionary<string, int>>();
            if (progress.unlockedAchievements == null) progress.unlockedAchievements = new List<string>();
            if (progress.equippedTitleId == null) progress.equippedTitleId = "Novato";
        }
        catch
        {
            progress = new PlayerProgress();
        }
    }

    public async void ResetAllProgress()
    {
        Debug.LogWarning("⚠️ BORRANDO PROGRESO...");
        progress = new PlayerProgress();
        SaveLocal();
        await SaveToCloudAsync();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}