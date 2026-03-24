public interface IHttpService
{
    Cysharp.Threading.Tasks.UniTask<string> GetTextAsync(string url);
    Cysharp.Threading.Tasks.UniTask<UnityEngine.Texture2D> GetTextureAsync(string url);
}