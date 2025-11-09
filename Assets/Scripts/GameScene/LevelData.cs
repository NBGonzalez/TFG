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
    public List<string> options;
    public string correctAnswer;
    public List<PairData> pairs;
}

[System.Serializable]
public class PairData
{
    public string left;
    public string right;
}