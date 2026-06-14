// MockLeaderboardProvider.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class MockLeaderboardProvider : ILeaderboardProvider
{
    // =========================================
    //  POOL DE NOMBRES (variados, no solo dev)
    // =========================================
    private readonly string[] botNames =
    {
        // Generales / creativos
        "Lucia_23", "Pablo_M", "IreneBio", "CarlosR", "Marta_99",
        "JorgeLab", "SofiaEst", "DiegoMat", "LauraQ", "RaulTech",
        "Elena_V", "PedroSci", "AlexData", "AndreaChem", "MarcosF",
        // Mas variados
        "NuriaCell", "AdrianFis", "ClaraMed", "DanielEco", "PatriSQL",
        "HugoGen", "AlbaNeuro", "IvanCalc", "RocioLab", "SergioAI",
        "BeatrizR", "OscarBio", "PaulaNet", "RubenMat", "TeresaDev"
    };

    // =========================================
    //  CONFIGURACION
    // =========================================
    private const int BOT_COUNT = 8;
    private const float ROTATION_HOURS = 12f; // Cada cuantas horas rotan algunos bots

    public async Task<List<LeaderboardEntry>> GetRanking()
    {
        List<LeaderboardEntry> ranking = new List<LeaderboardEntry>();

        await Task.Delay(400);

        // --- Datos del jugador ---
        var progress = PlayerProgressManager.Instance;
        int playerStars = progress != null ? progress.GetTotalStars() : 0;

        // --- Cargar todos los avatares y titulos disponibles ---
        ProfileAvatarSO[] allAvatars = Resources.LoadAll<ProfileAvatarSO>("Avatars");
        ProfileTitleSO[] allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        string[] avatarIds = allAvatars.Select(a => a.id).ToArray();
        string[] titleIds = allTitles.Select(t => t.id).ToArray();

        // Fallbacks por si no hay datos
        if (avatarIds.Length == 0) avatarIds = new[] { "avatar_default" };
        if (titleIds.Length == 0) titleIds = new[] { "Novato" };

        // --- Semilla basada en el dia (persistencia temporal) ---
        // Cambia cada ROTATION_HOURS horas, asi algunos bots rotan periodicamente
        int timeSeed = (int)(System.DateTime.UtcNow.Ticks / (System.TimeSpan.TicksPerHour * ROTATION_HOURS));

        // --- Generar bots con distribucion inteligente ---
        // La clave: las estrellas de los bots se basan en las del jugador
        // para que siempre tenga competencia cercana y alcanzable
        int[] botStarOffsets = CalculateBotDistribution(playerStars);

        for (int i = 0; i < BOT_COUNT; i++)
        {
            // Semilla unica por bot + periodo temporal
            // Algunos bots usan semilla par (cambian cada periodo)
            // Otros usan semilla impar (cambian en periodos alternos)
            // Esto hace que no todos cambien a la vez
            int botSeed = timeSeed * 1000 + i * 137 + (i % 3 == 0 ? timeSeed / 2 : 0);
            System.Random rng = new System.Random(botSeed);

            var bot = new LeaderboardEntry();

            // Nombre: determinista por semilla
            bot.userName = botNames[rng.Next(botNames.Length)];

            // Estrellas: basadas en la distribucion calculada + ligera variacion por semilla
            int baseStars = botStarOffsets[i];
            int variation = rng.Next(-1, 2); // -1, 0 o +1 de variacion
            bot.score = Mathf.Max(1, baseStars + variation);

            // Avatar y titulo: variados con semilla
            bot.avatarId = avatarIds[rng.Next(avatarIds.Length)];
            bot.titleId = titleIds[rng.Next(titleIds.Length)];

            bot.isMe = false;
            ranking.Add(bot);
        }

        // --- Anadir al jugador real ---
        if (progress != null)
        {
            var myEntry = new LeaderboardEntry();

            if (GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated())
                myEntry.userName = GooglePlayGames.PlayGamesPlatform.Instance.GetUserDisplayName();
            else
                myEntry.userName = "Tu (Invitado)";

            myEntry.score = playerStars;
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
    /// Genera una distribucion de estrellas para los bots basada en el progreso del jugador.
    ///
    /// PSICOLOGIA DEL APRENDIZAJE aplicada:
    ///
    /// 1. EFECTO NEAR-MISS: 2 bots estan justo por encima del jugador (+1 a +4 estrellas).
    ///    Esto genera la sensacion de "con un nivel mas les adelanto", motivando a seguir.
    ///
    /// 2. PROGRESO VISIBLE: 2 bots estan justo por debajo (-1 a -3 estrellas).
    ///    El jugador ve que ha superado a otros, reforzando su autoeficacia.
    ///
    /// 3. META ASPIRACIONAL ALCANZABLE: El bot top esta a +30-60% de las estrellas del jugador.
    ///    Es dificil pero NO imposible llegar al #1. Si el jugador juega mucho, puede lograrlo.
    ///    Pero como la semilla cambia cada 12h, el top puede "subir" ligeramente, haciendo
    ///    que mantener el #1 sea un reto continuo (no permanente).
    ///
    /// 4. ZONA DE CONFORT: 2 bots estan bastante por debajo (-30% a -50%).
    ///    Esto evita la frustracion total y da sensacion de comunidad activa.
    ///
    /// 5. ESCALADO POR 3: Como cada nivel da max 3 estrellas, las diferencias son multiplos
    ///    de 1-3 para que se sientan como "un nivel de diferencia".
    /// </summary>
    private int[] CalculateBotDistribution(int playerStars)
    {
        int[] offsets = new int[BOT_COUNT];

        // Caso especial: jugador nuevo (0-2 estrellas)
        if (playerStars <= 2)
        {
            offsets[0] = 6;  // Top aspiracional
            offsets[1] = 4;  // Reto medio
            offsets[2] = 3;  // Near-miss arriba
            offsets[3] = 2;  // Near-miss arriba
            offsets[4] = 1;  // Justo abajo (o igual)
            offsets[5] = 1;  // Justo abajo
            offsets[6] = 0;  // Abajo
            offsets[7] = 0;  // Abajo
            return offsets;
        }

        // Distribucion normal relativa al jugador
        // Top aspiracional: +30% a +60% (alcanzable con esfuerzo)
        offsets[0] = playerStars + Mathf.Max(3, Mathf.RoundToInt(playerStars * 0.50f));
        offsets[1] = playerStars + Mathf.Max(2, Mathf.RoundToInt(playerStars * 0.30f));

        // Near-miss arriba: +1 a +4 estrellas (efecto "casi les pillo")
        offsets[2] = playerStars + Mathf.Clamp(Mathf.RoundToInt(playerStars * 0.10f), 2, 4);
        offsets[3] = playerStars + 1;

        // Near-miss abajo: -1 a -3 estrellas (refuerzo positivo)
        offsets[4] = playerStars - 1;
        offsets[5] = playerStars - Mathf.Clamp(Mathf.RoundToInt(playerStars * 0.10f), 2, 3);

        // Zona de confort abajo: -30% a -50%
        offsets[6] = playerStars - Mathf.Max(2, Mathf.RoundToInt(playerStars * 0.35f));
        offsets[7] = playerStars - Mathf.Max(3, Mathf.RoundToInt(playerStars * 0.50f));

        // Asegurar minimo de 1 estrella para todos
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = Mathf.Max(1, offsets[i]);
        }

        return offsets;
    }
}