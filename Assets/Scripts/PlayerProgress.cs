using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    public Dictionary<string, List<string>> completedLevels = new Dictionary<string, List<string>>();
}
