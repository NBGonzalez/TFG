[System.Serializable]
public class LeaderboardEntry
{
    public int rank;            // 1, 2, 3...
    public string userName;     // "PacoGames"
    public int score;           // Estrellas: 450

    // IDs para buscar las imágenes luego
    public string avatarId;
    public string titleId;

    public bool isMe;           // ¿Soy yo? (Para pintarme de otro color)
}