using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface ISceneDirector
{
    UniTask<ISceneFlow> LoadAsync(SceneTypes sceneName, CancellationToken ct = default, IProgress<float> progress = null);
    UniTask UnloadAsync(SceneTypes sceneName, CancellationToken ct = default);
    bool IsLoaded(SceneTypes sceneName);
}
