public interface IContentService
{
    Cysharp.Threading.Tasks.UniTask<LevelsManifest> LoadManifestAsync();
    Cysharp.Threading.Tasks.UniTask<StoryPayload> LoadStoryAsync(string levelUrl);
}