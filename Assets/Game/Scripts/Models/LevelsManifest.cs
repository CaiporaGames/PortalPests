using System.Collections.Generic;

[System.Serializable]
public class LevelsManifest
{
    public int version;
    public string dictionaryUrl;
    public List<LevelIndexItem> levels;
}