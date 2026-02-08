using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq; // Necesario para ordenar listas (OrderByDescending)

public class MockLeaderboardProvider : ILeaderboardProvider
{
    // Nombres falsos para rellenar
    private readonly string[] botNames = { "Ana_Dev", "CodeWarrior", "PixelArt", "JavaFan", "UnityMaster", "BugHunter" };

    // Títulos posibles (ids)
    private readonly string[] possibleTitles = { "Novato", "sql_basic", "experto" }; // Asegúrate de que estos IDs existen en tus SO

    public async Task<List<LeaderboardEntry>> GetRanking()
    {
        List<LeaderboardEntry> ranking = new List<LeaderboardEntry>();

        // 1. Simular un pequeño tiempo de carga (como si conectara a internet)
        await Task.Delay(500); // 0.5 segundos

        // 2. Crear BOTS aleatorios
        foreach (string botName in botNames)
        {
            var bot = new LeaderboardEntry();
            bot.userName = botName;

            // Puntuación aleatoria (entre 10 y 1000 estrellas)
            bot.score = Random.Range(10, 1000);

            // Datos visuales aleatorios
            bot.avatarId = "avatar_default"; // O pon ids aleatorios si tienes más avatares
            bot.titleId = possibleTitles[Random.Range(0, possibleTitles.Length)];
            bot.isMe = false;

            ranking.Add(bot);
        }

        // 3. Añadir al JUGADOR REAL (Tú)
        // Obtenemos los datos reales del Manager
        var progress = PlayerProgressManager.Instance;
        if (progress != null)
        {
            var myEntry = new LeaderboardEntry();

            // Usamos el nombre de Google Play si hay, si no "Yo"
            if (GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated())
                myEntry.userName = GooglePlayGames.PlayGamesPlatform.Instance.GetUserDisplayName();
            else
                myEntry.userName = "Tú (Invitado)";

            myEntry.score = progress.GetTotalStars();
            myEntry.avatarId = progress.GetEquippedAvatarId();
            myEntry.titleId = progress.GetEquippedTitle();
            myEntry.isMe = true;

            ranking.Add(myEntry);
        }

        // 4. ORDENAR la lista (El que tiene más score va primero)
        // Usamos LINQ: OrderByDescending
        var sortedRanking = ranking.OrderByDescending(x => x.score).ToList();

        // 5. Asignar los RANGOS (#1, #2, #3...)
        for (int i = 0; i < sortedRanking.Count; i++)
        {
            sortedRanking[i].rank = i + 1; // +1 porque el índice empieza en 0
        }

        return sortedRanking;
    }
}