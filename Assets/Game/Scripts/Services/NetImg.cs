using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public static class NetImg
{
    // Downloads bytes, then decodes to a Texture2D.
    public static async UniTask<Texture2D> GetTextureViaBytesAsync(string url, bool nonReadable = false)
    {
        using var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();

        try
        {
            await req.SendWebRequest(); // UniTask will throw on network error
        }
        catch (UnityWebRequestException ex)
        {
            Debug.LogError($"[GetTextureViaBytesAsync] Network error for {url}\n{ex}");
            throw;
        }

        if (req.result != UnityWebRequest.Result.Success)
            throw new System.Exception($"HTTP {(int)req.responseCode} {req.error} for {url}");

        var data = req.downloadHandler.data;
        if (data == null || data.Length == 0)
            throw new System.Exception($"No data returned for {url}");

        // Decode image bytes
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        var ok = ImageConversion.LoadImage(tex, data, markNonReadable: nonReadable);
        if (!ok)
            throw new System.Exception($"Image decode failed (not a valid image?): {url}");

        return tex;
    }
}
