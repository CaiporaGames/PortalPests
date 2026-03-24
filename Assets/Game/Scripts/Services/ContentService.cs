using Cysharp.Threading.Tasks;
using UnityEngine;

public class ContentService : IContentService 
{
    private readonly ITitleDataService titleData;
    private readonly IHttpService http;
    private LevelsManifest cachedManifest;

    public ContentService(ITitleDataService titleData, IHttpService http) {
        this.titleData = titleData; this.http = http;
    }

    public async UniTask<LevelsManifest> LoadManifestAsync() {
        if (cachedManifest != null) return cachedManifest;
        var url = await titleData.GetContentIndexUrlAsync();
        var json = await http.GetTextAsync(url);
        cachedManifest = JsonUtility.FromJson<LevelsManifest>(json);
        return cachedManifest;
    }

    public async UniTask<StoryPayload> LoadStoryAsync(string levelUrl) {
        var json = await http.GetTextAsync(levelUrl);
        return JsonUtility.FromJson<StoryPayload>(json);
    }
}