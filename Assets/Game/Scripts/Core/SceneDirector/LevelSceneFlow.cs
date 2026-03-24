using UnityEngine;
using Cysharp.Threading.Tasks;


public class LevelSceneFlow : MonoBehaviour, ISceneFlow
{
    [Header("Scene-local refs/prefabs")]

    private GameContext _ctx;
    private GameObject _playerInstance;

    public async UniTask InitializeAsync(GameContext ctx)
    {
        _ctx = ctx;
        Debug.Log($"Initializing LevelSceneFlow for scene: {gameObject.scene.name}");
        await _ctx.UI.ShowScreenAsync<object>(ScreenTypes.LevelLoaderScreen);
    }

    public async UniTask ShutdownAsync()
    {
        // Unsubscribe, stop coroutines, return pooled objects, etc.
        if (_playerInstance != null) Destroy(_playerInstance);
        await UniTask.CompletedTask;
    }

    // Optional: helper to exit from a button in this scene
   /*  public async void OnClickExit()
    {
        // Find my own scene name and unload via director
        var sceneName = gameObject.scene.name;
        await ShutdownAsync();
        var director = ServiceLocator.Resolve<ISceneDirector>();
        await director.UnloadAsync(sceneName);
        // Optionally show a hub screen again:
        await _ctx.UI.ShowScreenAsync<object>(ScreenTypes.LevelDetailScreen);
    } */
}
