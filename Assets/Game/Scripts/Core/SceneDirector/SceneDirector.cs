using UnityEngine.SceneManagement;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;

public sealed class SceneDirector : ISceneDirector
{
    private readonly HashSet<string> _loading = new();

    public bool IsLoaded(SceneTypes sceneName) => SceneManager.GetSceneByName(sceneName.ToString()).isLoaded;


    public async UniTask<ISceneFlow> LoadAsync(SceneTypes sceneName, CancellationToken ct = default, IProgress<float> progress = null)
    {
        if (_loading.Contains(sceneName.ToString())) return null; // guard re-entry
        _loading.Add(sceneName.ToString());
        try
        {
            var op = SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
            while (!op.isDone)
            {
                progress?.Report(op.progress);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            var scene = SceneManager.GetSceneByName(sceneName.ToString());
            foreach (var root in scene.GetRootGameObjects())
            {
                var flow = root.GetComponentInChildren<ISceneFlow>(true);
                if (flow != null) return flow;
            }
            Debug.LogWarning($"No ISceneFlow found in scene {sceneName}.");
            return null;
        }
        finally { _loading.Remove(sceneName.ToString()); }
    }

    public async UniTask UnloadAsync(SceneTypes sceneName, CancellationToken ct = default)
    {
        if (!IsLoaded(sceneName)) return;
        var op = SceneManager.UnloadSceneAsync(sceneName.ToString());
        while (op != null && !op.isDone)
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
    }
}
