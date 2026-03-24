using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class UnityHttpService : IHttpService
{
    public async UniTask<string> GetTextAsync(string url)
    {
        using var req = UnityWebRequest.Get(url);
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) throw new System.Exception(req.error);
        return req.downloadHandler.text;
    }
    public async UniTask<Texture2D> GetTextureAsync(string url)
    {
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();

        try { await req.SendWebRequest(); }
        catch (UnityWebRequestException ex)
        {
            Debug.LogError($"[GetTextureViaBytesAsync] {url}\n{ex}");
            throw;
        }

        if (req.result != UnityWebRequest.Result.Success)
            throw new System.Exception($"HTTP {(int)req.responseCode} {req.error} for {url}");

        var data = req.downloadHandler.data;
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(tex, data))
            throw new System.Exception($"Image decode failed. Maybe not an image? URL: {url}");

        return tex;
    }
}