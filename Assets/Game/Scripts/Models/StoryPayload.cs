using System.Collections.Generic;

[System.Serializable]
public class StoryPayload
{
    public string id, title, description;
    public List<StorySection> story;
}