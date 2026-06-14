// MockLeaderboardProvider.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class MockLeaderboardProvider : ILeaderboardProvider
{
    // =========================================
    //  POOL DE NOMBRES (variados)
    // =========================================
    private readonly string[] botNames =
    {
        "Lucia_23", "Pablo_M", "IreneBio", "CarlosR", "Marta_99",
        "JorgeLab", "SofiaEst", "DiegoMat", "LauraQ", "RaulTech",
        "Elena_V", "PedroSci", "AlexData", "AndreaChem", "MarcosF",
        "NuriaCell", "AdrianFis", "ClaraMed", "DanielEco", "PatriSQL",
        "HugoGen", "AlbaNeuro", "IvanCalc", "RocioLab", "SergioAI",
        "BeatrizR", "OscarBio", "PaulaNet", "RubenMat", "TeresaDev"
    };

    // =========================================
    //  CONFIGURACION
    // =========================================
    private const int BOT_COUNT = 8;

    // Offsets ABSOLUTOS de estrellas respecto a la base del jugador.
    // Basados en que un jugador activo gana ~25 estrellas/dia maximo.
    // El top esta a +18, alcanzable jugando bien un dia completo.
    private readonly int[] botOffsets = { 18, 12, 7, 3, -2, -6, -12, -18 };

    // Claves de PlayerPrefs para persistencia
    private const string PREF_BASE_STARS = "LeaderboardBaseStars";
    private const string PREF_BASE_DATE = "LeaderboardBaseDate";

    public async Task<List<LeaderboardEntry>> GetRanking()
    {
        List<LeaderboardEntry> ranking = new List<LeaderboardEntry>();

        await Task.Delay(400);

        // --- Datos del jugador ---
        var progress = PlayerProgressManager.Instance;
        int playerStarsNow = progress != null ? progress.GetTotalStars() : 0;

        // --- Obtener la base congelada del dia ---
        int baseStars = GetOrUpdateDailyBase(playerStarsNow);

        // --- Semilla del dia (para nombres, avatares, titulos) ---
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
        int daySeed = today.GetHashCode();

        // --- Cargar avatares y titulos disponibles ---
        ProfileAvatarSO[] allAvatars = Resources.LoadAll<ProfileAvatarSO>("Avatars");
        ProfileTitleSO[] allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        string[] avatarIds = allAvatars.Select(a => a.id).ToArray();
        string[] titleIds = allTitles.Select(t => t.id).ToArray();

        if (avatarIds.Length == 0) avatarIds = new[] { "avatar_default" };
        if (titleIds.Length == 0) titleIds = new[] { "Novato" };

        // --- Generar bots ---
        for (int i = 0; i < BOT_COUNT; i++)
        {
            // Semilla unica por bot + dia (determinista dentro del mismo dia)
            // Cada bot tiene un offset de semilla diferente para que no todos
            // cambien el mismo dia: algunos cambian en dias pares, otros en impares
            int botDaySeed = daySeed + i * 7919;
            if (i % 3 == 0) botDaySeed += (daySeed / 3); // Variacion extra para algunos
            System.Random rng = new System.Random(botDaySeed);

            var bot = new LeaderboardEntry();

            // Nombre determinista
            bot.userName = botNames[rng.Next(botNames.Length)];

            // Estrellas: base congelada + offset fijo + pequena variacion diaria
            int dailyVariation = rng.Next(0, 4); // 0-3 estrellas de variacion
            int botScore = baseStars + botOffsets[i] + dailyVariation;
            bot.score = Mathf.Max(1, botScore);

            // Avatar y titulo variados
            bot.avatarId = avatarIds[rng.Next(avatarIds.Length)];
            bot.titleId = titleIds[rng.Next(titleIds.Length)];

            bot.isMe = false;
            ranking.Add(bot);
        }

        // --- Anadir al jugador real (con estrellas ACTUALES, no la base) ---
        if (progress != null)
        {
            var myEntry = new LeaderboardEntry();

            if (GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated())
                myEntry.userName = GooglePlayGames.PlayGamesPlatform.Instance.GetUserDisplayName();
            else
                myEntry.userName = "Tu (Invitado)";

            myEntry.score = playerStarsNow; // Estrellas en tiempo real
            myEntry.avatarId = progress.GetEquippedAvatarId();
            myEntry.titleId = progress.GetEquippedTitle();
            myEntry.isMe = true;
            ranking.Add(myEntry);
        }

        // --- Ordenar y asignar rangos ---
        var sorted = ranking.OrderByDescending(x => x.score).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].rank = i + 1;
        }

        return sorted;
    }

    /// <summary>
    /// Obtiene las estrellas base del dia actual.
    /// Si es un dia nuevo, actualiza la base con las estrellas actuales del jugador.
    /// Los bots se calculan sobre esta base CONGELADA, asi que mientras el jugador
    /// gana estrellas durante el dia, SUBE en el ranking.
    /// Al dia siguiente, la base se actualiza y los bots "avanzan" tambien.
    /// </summary>
    private int GetOrUpdateDailyBase(int currentPlayerStars)
    {
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
        string storedDate = PlayerPrefs.GetString(PREF_BASE_DATE, "");

        if (storedDate != today)
        {
            // Nuevo dia: congelar las estrellas actuales como base
            PlayerPrefs.SetInt(PREF_BASE_STARS, currentPlayerStars);
            PlayerPrefs.SetString(PREF_BASE_DATE, today);
            PlayerPrefs.Save();

            Debug.Log($"[Leaderboard] Nuevo dia. Base congelada: {currentPlayerStars} estrellas");
            return currentPlayerStars;
        }

        // Mismo dia: devolver la base congelada
        return PlayerPrefs.GetInt(PREF_BASE_STARS, currentPlayerStars);
    }
}