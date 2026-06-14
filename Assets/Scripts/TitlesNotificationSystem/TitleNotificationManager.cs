// TitleNotificationManager.cs
using UnityEngine;
using System.Collections.Generic;

public class TitleNotificationManager : MonoBehaviour
{
    public static TitleNotificationManager Instance { get; private set; }

    private Queue<ProfileTitleSO> pendingNotifications = new Queue<ProfileTitleSO>();

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

    public void EnqueueNotification(ProfileTitleSO titleData)
    {
        if (titleData != null)
        {
            pendingNotifications.Enqueue(titleData);
            Debug.Log($"[TitleNotification] Notificacion encolada: {titleData.titleName}");
        }
    }

    public bool TryDequeue(out ProfileTitleSO titleData)
    {
        if (pendingNotifications.Count > 0)
        {
            titleData = pendingNotifications.Dequeue();
            return true;
        }
        titleData = null;
        return false;
    }

    /// <summary>
    /// Comprueba todos los titulos y desbloquea los que cumplen requisitos.
    /// Los que sean nuevos se encolan como notificacion.
    /// Llamar al cargar la MainScene para detectar desbloqueos de otras escenas.
    /// </summary>
    public void CheckForNewUnlocks()
    {
        var progress = PlayerProgressManager.Instance;
        if (progress == null) return;

        ProfileTitleSO[] allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        foreach (var titleData in allTitles)
        {
            // Si ya estaba desbloqueado, no es nuevo
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
                EnqueueNotification(titleData);
            }
        }
    }
}