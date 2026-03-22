using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string language;
    public string levelId;
    public string levelTitle;
    public List<MiniGameData> minigames;
}

[System.Serializable]
public class MiniGameData
{
    public string type; // "Explain", "Quizz", "Arrows", "FillBlanks"

    public string title;
    public string content;
    public string question;
    public string instruction;

    public List<string> options;       // Para Quizz y FillBlanks
    public string correctAnswer;       // Para Quizz (FillBlanks ya no usa esto)

    public List<PairData> pairs;       // Para Arrows
    
    public List<FillBlankEntry> blanks;// Para FillBlanks

    public List<string> images;        // Opcional: rutas de imágenes a mostrar (pueden ser usadas por cualquier tipo de minijuego)
}

[System.Serializable]
public class FillBlankEntry
{
    public int id;          // número del hueco (1,2,3...)
    public string correct;  // texto correcto
}


[System.Serializable]
public class PairData
{
    public string left;
    public string right;
}