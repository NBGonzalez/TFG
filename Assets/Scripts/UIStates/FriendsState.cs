using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class FriendsState : UIStateBase
{
    [Header("Referencias")]
    [SerializeField] private Button backButton;
    [SerializeField] private Transform listContent;
    [SerializeField] private GameObject leaderboardSlotPrefab;
    [SerializeField] private GameObject loadingSpinner;

    [Header("Temporizador de Reset")]
    [SerializeField] private TextMeshProUGUI resetTimerText;

    [Header("Iconos de Trofeo (para notificaciones)")]
    [SerializeField] private Sprite trophyGold;
    [SerializeField] private Sprite trophySilver;
    [SerializeField] private Sprite trophyBronze;

    // Datos estaticos para "traducir" IDs a Imagenes
    private ProfileAvatarSO[] allAvatars;
    private ProfileTitleSO[] allTitles;

    // Para el temporizador
    private Coroutine timerCoroutine;

    // Claves de PlayerPrefs para las recompensas
    private const string PREF_REWARD_DATE = "LeaderboardRewardDate";
    private const string PREF_LAST_RANK = "LeaderboardLastRank";

    public override void OnEnter()
    {
        base.OnEnter();
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));

        allAvatars = Resources.LoadAll<ProfileAvatarSO>("Avatars");
        allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        // Comprobar si hay recompensa pendiente del dia anterior
        CheckAndGrantReward();

        // Generar el Ranking
        RefreshLeaderboard();

        // Iniciar el temporizador
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(UpdateTimerRoutine());
    }

    // =========================================
    //  RECOMPENSA POR POSICION
    // =========================================
    private void CheckAndGrantReward()
    {
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
        string lastRewardDate = PlayerPrefs.GetString(PREF_REWARD_DATE, "");

        if (lastRewardDate == today) return;

        int lastRank = PlayerPrefs.GetInt(PREF_LAST_RANK, 0);

        if (lastRank > 0 && lastRewardDate != "" && lastRewardDate != today)
        {
            int reward = GetRewardForRank(lastRank);
            if (reward > 0 && PlayerProgressManager.Instance != null)
            {
                PlayerProgressManager.Instance.AddStars(reward);

                // Encolar notificacion con el trofeo correspondiente
                if (TitleNotificationManager.Instance != null)
                {
                    Sprite trophy = GetTrophyForRank(lastRank);
                    TitleNotificationManager.Instance.EnqueueLeaderboardReward(lastRank, reward, trophy);
                }

                Debug.Log($"[Leaderboard] Recompensa por quedar #{lastRank}: +{reward} estrellas!");
            }
        }

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

    // =========================================
    //  LEADERBOARD
    // =========================================
    private async void RefreshLeaderboard()
    {
        foreach (Transform child in listContent) Destroy(child.gameObject);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        ILeaderboardProvider provider = new MockLeaderboardProvider();
        List<LeaderboardEntry> data = await provider.GetRanking();

        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        foreach (var entry in data)
        {
            if (entry.isMe)
            {
                PlayerPrefs.SetInt(PREF_LAST_RANK, entry.rank);
                PlayerPrefs.Save();
                break;
            }
        }

        foreach (var entry in data)
        {
            GameObject newSlot = Instantiate(leaderboardSlotPrefab, listContent);
            UI_LeaderboardSlot slotScript = newSlot.GetComponent<UI_LeaderboardSlot>();

            ProfileAvatarSO avatarData = GetAvatarById(entry.avatarId);
            ProfileTitleSO titleData = GetTitleById(entry.titleId);

            slotScript.Setup(entry, avatarData, titleData);
        }
    }

    // =========================================
    //  TEMPORIZADOR DE RESET
    // =========================================
    private IEnumerator UpdateTimerRoutine()
    {
        while (true)
        {
            if (resetTimerText != null)
            {
                System.DateTime nowUtc = System.DateTime.UtcNow;
                System.DateTime nextReset = nowUtc.Date.AddDays(1);
                System.TimeSpan remaining = nextReset - nowUtc;

                resetTimerText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }

            yield return new WaitForSeconds(1f);
        }
    }

    // =========================================
    //  HELPERS
    // =========================================
    private ProfileAvatarSO GetAvatarById(string id)
    {
        if (allAvatars == null) return null;
        foreach (var a in allAvatars) if (a.id == id) return a;
        return null;
    }

    private ProfileTitleSO GetTitleById(string id)
    {
        if (allTitles == null) return null;
        foreach (var t in allTitles) if (t.id == id) return t;
        return null;
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }
}