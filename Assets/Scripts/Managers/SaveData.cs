using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int lastUnlockedStage = 1;
    public List<int> starredProgressStages = new List<int>();
}
