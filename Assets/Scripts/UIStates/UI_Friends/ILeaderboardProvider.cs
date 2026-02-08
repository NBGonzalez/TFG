using System.Collections.Generic;
using System.Threading.Tasks; // Necesario para operaciones asíncronas (red)

public interface ILeaderboardProvider
{
    // Devuelve una Tarea (Task) que en el futuro nos dará una Lista de entradas
    Task<List<LeaderboardEntry>> GetRanking();
}
