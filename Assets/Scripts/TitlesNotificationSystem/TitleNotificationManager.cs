// TitleNotificationManager.cs
using UnityEngine;
using System.Collections.Generic;

public class TitleNotificationManager : MonoBehaviour
{
    public static TitleNotificationManager Instance { get; private set; }

    [Header("Iconos de Trofeo (para notificaciones de leaderboard)")]
    [SerializeField] private Sprite trophyGold;
    [SerializeField] private Sprite trophySilver;
    [SerializeField] private Sprite trophyBronze;

    // Cola generica de notificaciones
    private Queue<NotificationData> pendingNotifications = new Queue<NotificationData>();

    // Claves de PlayerPrefs para las recompensas del leaderboard
    private const string PREF_REWARD_DATE = "LeaderboardRewardDate";
    private const string PREF_LAST_RANK = "LeaderboardLastRank";

    public bool HasPending => pendingNotifications.Count > 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================
    //  API GENERICA
    // =========================================

    public void Enqueue(NotificationData data)
    {
        if (data != null)
        {
            pendingNotifications.Enqueue(data);
            Debug.Log($"[Notification] Encolada: {data.defaultText} - {data.titleText}");
        }
    }

    public bool TryDequeue(out NotificationData data)
    {
        if (pendingNotifications.Count > 0)
        {
            data = pendingNotifications.Dequeue();
            return true;
        }
        data = null;
        return false;
    }

    // =========================================
    //  HELPERS DE CONVENIENCIA
    // =========================================

    public void EnqueueTitleUnlocked(ProfileTitleSO titleData)
    {
        if (titleData == null) return;
        Enqueue(new NotificationData(
            icon: titleData.icon,
            defaultText: "\u00a1Has desbloqueado!",
            titleText: titleData.titleName,
            targetState: "Profile"
        ));
    }

    public void EnqueueLeaderboardReward(int rank, int starsEarned, Sprite trophyIcon)
    {
        Enqueue(new NotificationData(
            icon: trophyIcon,
            defaultText: $"\u00a1Has quedado en el top {rank} del ranking de ayer!",
            titleText: $"Como recompensa has obtenido {starsEarned} estrellas.",
            targetState: "Friends"
        ));
    }

    // =========================================
    //  CHECK DE TITULOS
    // =========================================
    public void CheckForNewUnlocks()
    {
        var progress = PlayerProgressManager.Instance;
        if (progress == null) return;

        ProfileTitleSO[] allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        foreach (var titleData in allTitles)
        {
            if (progress.HasUnlocked(titleData.id)) continue;

            bool unlocked = false;
            switch (titleData.requirementType)
            {
                case UnlockRequirementType.None:
                    unlocked = true;
                    break;
                case UnlockRequirementType.TotalStars:
                    unlocked = progress.GetTotalStars() >= titleData.requirementValue;
                    break;
                case UnlockRequirementType.StreakDays:
                    unlocked = progress.GetStreak() >= titleData.requirementValue;
                    break;
                case UnlockRequirementType.SQLLevelPassed:
                    unlocked = progress.IsLevelCompleted("SQL", $"sql-{titleData.requirementValue}");
                    break;
                case UnlockRequirementType.BiologiaLevelPassed:
                    unlocked = progress.IsLevelCompleted("Biologia", $"biologia-{titleData.requirementValue}");
                    break;
            }

            if (unlocked)
            {
                progress.UnlockAchievement(titleData.id);
                EnqueueTitleUnlocked(titleData);
            }
        }
    }

    // =========================================
    //  CHECK DE RECOMPENSA LEADERBOARD
    // =========================================

    /// <summary>
    /// Comprueba si el jugador tiene una recompensa pendiente del dia anterior.
    /// Se llama desde UIStateManager.Start() al cargar la MainScene.
    /// </summary>
    public void CheckLeaderboardReward()
    {
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
        string lastRewardDate = PlayerPrefs.GetString(PREF_REWARD_DATE, "");

        // Si ya recibimos recompensa hoy, no hacer nada
        if (lastRewardDate == today) return;

        int lastRank = PlayerPrefs.GetInt(PREF_LAST_RANK, 0);

        if (lastRank > 0 && lastRewardDate != "" && lastRewardDate != today)
        {
            int reward = GetRewardForRank(lastRank);
            if (reward > 0 && PlayerProgressManager.Instance != null)
            {
                PlayerProgressManager.Instance.AddStars(reward);

                Sprite trophy = GetTrophyForRank(lastRank);
                EnqueueLeaderboardReward(lastRank, reward, trophy);

                Debug.Log($"[Leaderboard] Recompensa por quedar #{lastRank}: +{reward} estrellas!");
            }
        }

        // Marcar que ya hemos procesado la recompensa de hoy
        PlayerPrefs.SetString(PREF_REWARD_DATE, today);
        PlayerPrefs.Save();
    }

    private int GetRewardForRank(int rank)
    {
        switch (rank)
        {
            case 1: return 20;
            case 2: return 10;
            case 3: return 5;
            default: return 0;
        }
    }

    private Sprite GetTrophyForRank(int rank)
    {
        switch (rank)
        {
            case 1: return trophyGold;
            case 2: return trophySilver;
            case 3: return trophyBronze;
            default: return null;
        }
    }
}