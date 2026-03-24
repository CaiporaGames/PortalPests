using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BinarySaveService : ISaveService
{
    string GetPath(SaveType key) => Path.Combine(Application.persistentDataPath, key + ".bin");

    public async UniTask<T> LoadAsync<T>(SaveType key) where T: IBinarySerializable, new()
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return default;
        byte[] bytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(path));

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);
        var obj = new T(); obj.Deserialize(br); return obj;
    }

    public async UniTask SaveAsync<T>(SaveType key, T data) where T: IBinarySerializable
    {
        var path = GetPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        byte[] bytes;
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        { data.Serialize(bw); bw.Flush(); bytes = ms.ToArray(); }
        await UniTask.RunOnThreadPool(() => File.WriteAllBytes(path, bytes));
    }
}