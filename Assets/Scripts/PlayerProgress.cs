using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    // --- JUEGO ---
    public Dictionary<string, List<string>> completedLevels = new Dictionary<string, List<string>>();

    // --- PERFIL ---
    public string equippedTitleId = "Novato";

    // NUEVO: El ID del avatar que llevamos puesto
    public string currentAvatarId = "avatar_default";

    public List<string> unlockedAchievements = new List<string>();

    // --- STATS ---
    public int totalStars = 0;
    public int currentStreak = 0;
    public string lastLoginDate = "";
}
