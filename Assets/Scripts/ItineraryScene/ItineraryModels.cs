using System;
using System.Collections.Generic;

// --- ESTRUCTURA DEL MEGA-JSON ---

[Serializable]
public class CustomItineraryData
{
    public string itineraryId;
    public string language;
    public string title;
    public string description;

    public string authorName;

    // Lista de niveles que componen el itinerario
    public List<CustomLevelData> levels = new List<CustomLevelData>();
}

[Serializable]
public class CustomLevelData
{
    public string levelId;
    public string levelTitle;
    public string levelDescription;

    // Reutilizamos tu clase MiniGameData original para los minijuegos
    public List<MiniGameData> minigames = new List<MiniGameData>();
}

// Nota: No incluyo aquí MiniGameData ni FillBlankEntry porque 
// tú ya los tienes definidos en tu script 'LevelData.cs'.
// Unity los reconocerá automáticamente.
