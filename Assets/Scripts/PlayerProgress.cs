//PlayerProgress.cs

using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    // --- JUEGO (SINGLE SOURCE OF TRUTH) ---
    // Diccionario único. Si un nivel está aquí, es que está completado.
    // Clave 1: Lenguaje ("SQL") -> Clave 2: Nivel ("sql-1") -> Valor: Estrellas (1, 2 o 3)
    public Dictionary<string, Dictionary<string, int>> levelStars = new Dictionary<string, Dictionary<string, int>>();

    // --- PERFIL ---
    public string equippedTitleId = "Novato";
    public string currentAvatarId = "avatar_default";
    public List<string> unlockedAchievements = new List<string>();

    // --- STATS ---
    public int totalStars = 0;
    public int currentStreak = 0;
    public string lastLoginDate = "";
}