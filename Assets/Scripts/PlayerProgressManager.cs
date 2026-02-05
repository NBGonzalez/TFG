using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;

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
            // Protegemos esto por si Instance es null al cerrar
            if (AuthenticationService.Instance != null)
                AuthenticationService.Instance.SignedIn -= OnUserSignedIn;
        }
        catch { }
    }

    private async void OnUserSignedIn()
    {
        Debug.Log("[Progress] Usuario detectado. Intentando descargar datos de la nube...");
        await LoadFromCloudAsync();

        // --- NUEVO: Al iniciar sesión, comprobamos la racha ---
        CheckDailyStreak();
    }

    // =============================
    //  PUBLIC API (LO VIEJO + LO NUEVO)
    // =============================

    // --- LÓGICA ANTIGUA (Niveles) ---
    public bool IsLevelCompleted(string language, string levelId)
    {
        if (progress.completedLevels.ContainsKey(language))
        {
            return progress.completedLevels[language].Contains(levelId);
        }
        return false;
    }

    public async void CompleteLevel(string language, string levelId)
    {
        Debug.Log($"[PlayerProgressManager] Marcando nivel como completado: {language} - {levelId}");
        if (!progress.completedLevels.ContainsKey(language))
            progress.completedLevels[language] = new List<string>();

        if (!progress.completedLevels[language].Contains(levelId))
        {
            progress.completedLevels[language].Add(levelId);

            // Aquí podrías sumar estrellas automáticamente si quisieras
            // AddStars(1); 

            SaveLocal();
            await SaveToCloudAsync();
        }
    }

    public List<string> GetCompletedLevels(string language)
    {
        if (!progress.completedLevels.ContainsKey(language))
            return new List<string>();

        return new List<string>(progress.completedLevels[language]);
    }

    // --- LÓGICA NUEVA (Perfil, Rachas, Estrellas) ---

    public void CheckDailyStreak()
    {
        if (string.IsNullOrEmpty(progress.lastLoginDate))
        {
            progress.currentStreak = 1;
            UpdateLastLoginDate();
            // Guardamos local para que no se pierda si cierra rápido
            SaveLocal();
            return;
        }

        System.DateTime lastDate;
        // TryParse es más seguro por si el string viene corrupto
        if (!System.DateTime.TryParse(progress.lastLoginDate, out lastDate))
        {
            UpdateLastLoginDate();
            return;
        }

        System.DateTime today = System.DateTime.Now.Date;
        double daysDiff = (today - lastDate).TotalDays;

        if (daysDiff < 1)
        {
            return; // Mismo día
        }
        else if (daysDiff >= 1 && daysDiff < 2)
        {
            progress.currentStreak++; // Día consecutivo
            Debug.Log($"[Streak] ¡Racha aumentada a {progress.currentStreak}!");
        }
        else
        {
            progress.currentStreak = 1; // Racha perdida
            Debug.Log("[Streak] Racha perdida. Reinicio a 1.");
        }

        UpdateLastLoginDate();
        SaveLocal();
        // Nota: El guardado en nube se hará cuando complete nivel o salga, para no saturar.
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
        // Validación simple: o es "Novato" (por defecto) o lo tienes desbloqueado
        if (titleId == "Novato" || progress.unlockedAchievements.Contains(titleId))
        {
            progress.equippedTitleId = titleId;
            SaveLocal();
        }
    }

    public bool HasUnlocked(string achievementId)
    {
        // Si la lista es nula por lo que sea, devolvemos false para evitar errores
        if (progress == null || progress.unlockedAchievements == null) return false;

        return progress.unlockedAchievements.Contains(achievementId);
    }

    // --- GESTIÓN DE AVATARES ---
    public void EquipAvatar(string avatarId)
    {
        progress.currentAvatarId = avatarId;
        SaveLocal();
        _ = SaveToCloudAsync(); // Guardamos en nube para que se vea en otros dispositivos
    }

    public string GetEquippedAvatarId()
    {
        // Seguridad: si es nulo, devolvemos uno por defecto
        if (string.IsNullOrEmpty(progress.currentAvatarId)) return "avatar_default";
        return progress.currentAvatarId;
    }

    // Getters para la UI
    public int GetTotalStars() => progress.totalStars;
    public int GetStreak() => progress.currentStreak;
    public string GetEquippedTitle() => progress.equippedTitleId;


    // =============================
    // SAVE / LOAD (INTACTOS, SOLO AÑADIDO UN NULL CHECK)
    // =============================
    private async Task SaveToCloudAsync()
    {
        try
        {
            var dataToSave = new Dictionary<string, object> { { CLOUD_KEY, progress } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log("<color=green>[Cloud] Progreso guardado OK.</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cloud] Error save: {e.Message}");
        }
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
                    // FUSIÓN DE NIVELES (Tu lógica original)
                    foreach (var langEntry in cloudProgress.completedLevels)
                    {
                        string language = langEntry.Key;
                        List<string> levels = langEntry.Value;

                        if (!progress.completedLevels.ContainsKey(language))
                            progress.completedLevels[language] = new List<string>();

                        foreach (var lvl in levels)
                        {
                            if (!progress.completedLevels[language].Contains(lvl))
                                progress.completedLevels[language].Add(lvl);
                        }
                    }

                    // FUSIÓN DE STATS (Criterio: Nos quedamos con el mayor valor)
                    // Esto evita que si juegas offline y subes racha, la nube te la baje al sincronizar.
                    if (cloudProgress.totalStars > progress.totalStars)
                        progress.totalStars = cloudProgress.totalStars;

                    if (cloudProgress.currentStreak > progress.currentStreak)
                        progress.currentStreak = cloudProgress.currentStreak;

                    // Logros: fusionar listas sin duplicados
                    foreach (var ach in cloudProgress.unlockedAchievements)
                    {
                        if (!progress.unlockedAchievements.Contains(ach))
                            progress.unlockedAchievements.Add(ach);
                    }
                }

                Debug.Log("<color=cyan>[Cloud] Sync completada.</color>");
                SaveLocal();
                await SaveToCloudAsync();
            }
            else
            {
                Debug.Log("[Cloud] No hay datos en nube. Manteniendo local o iniciando nuevo.");
                if (progress != null && progress.completedLevels.Count > 0)
                    await SaveToCloudAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cloud] Error load: {e.Message}");
            // Si falla la nube, aseguramos que al menos 'progress' no sea null para que el juego funcione offline
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

            // Null checks de seguridad (por si el json es viejo y le faltan campos nuevos)
            if (progress.completedLevels == null) progress.completedLevels = new Dictionary<string, List<string>>();
            if (progress.unlockedAchievements == null) progress.unlockedAchievements = new List<string>();
            if (progress.equippedTitleId == null) progress.equippedTitleId = "Novato";
        }
        catch
        {
            progress = new PlayerProgress();
        }
    }
}