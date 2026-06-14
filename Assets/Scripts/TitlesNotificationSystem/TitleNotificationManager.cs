// TitleNotificationManager.cs
using UnityEngine;
using System.Collections.Generic;

public class TitleNotificationManager : MonoBehaviour
{
    public static TitleNotificationManager Instance { get; private set; }

    // Cola generica de notificaciones
    private Queue<NotificationData> pendingNotifications = new Queue<NotificationData>();

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

    /// <summary>
    /// Encola cualquier tipo de notificacion.
    /// </summary>
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

    /// <summary>
    /// Atajo para encolar una notificacion de titulo desbloqueado.
    /// </summary>
    public void EnqueueTitleUnlocked(ProfileTitleSO titleData)
    {
        if (titleData == null) return;

        var data = new NotificationData(
            icon: titleData.icon,
            defaultText: "¡Has desbloqueado!",
            titleText: titleData.titleName,
            targetState: "Profile"
        );
        Enqueue(data);
    }

    /// <summary>
    /// Atajo para encolar una notificacion de recompensa del leaderboard.
    /// </summary>
    public void EnqueueLeaderboardReward(int rank, int starsEarned, Sprite trophyIcon)
    {
        var data = new NotificationData(
            icon: trophyIcon,
            defaultText: $"¡Has quedado en el top {rank} del ranking de ayer!",
            titleText: $"Como recompensa has obtenido {starsEarned} estrellas.",
            targetState: "Friends"
        );
        Enqueue(data);
    }

    // =========================================
    //  CHECK DE TITULOS (ya existente)
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
}