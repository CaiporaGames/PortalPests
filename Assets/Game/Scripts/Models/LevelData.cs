using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public string id;
    public string title;
    public string description;
    public int x;
    public int y;
    public string thumbnail;
    public List<StorySection> story;
}