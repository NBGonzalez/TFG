// LevelModel.cs
[System.Serializable]
public class LevelModel
{
    public string id;
    public string title;
    public string description;
    //public string requiredLevel;
}

[System.Serializable]
public class PathModel
{
    public string language;
    public LevelModel[] levels;
}
