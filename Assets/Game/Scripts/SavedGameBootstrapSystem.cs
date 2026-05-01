using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavedGameBootstrapSystem : MonoBehaviour, IGameSystem
{
    [SerializeField] private string defaultSceneName = "Level 1";

    private PlayerLocationSaveManager _locationSaveManager;

    public bool IsInitialized { get; private set; }

    public async UniTask InitializeAsync()
    {
        if (IsInitialized)
            return;

        _locationSaveManager = ServiceLocator.Resolve<PlayerLocationSaveManager>();

        if (_locationSaveManager == null)
        {
            Debug.LogError("PlayerLocationSaveManager not found.");
            return;
        }

        if (!_locationSaveManager.IsInitialized)
            await _locationSaveManager.InitializeAsync();

        var data = _locationSaveManager.GetData();

        string sceneToLoad =
            data != null &&
            data.hasSavedLocation &&
            !string.IsNullOrWhiteSpace(data.sceneName)
                ? data.sceneName
                : defaultSceneName;

        await SceneManager.LoadSceneAsync(sceneToLoad);

        await UniTask.Yield();
        await UniTask.DelayFrame(1);

        RestorePlayerPosition(data);

        IsInitialized = true;
    }

    private void RestorePlayerPosition(PlayerLocationData data)
    {
        if (data == null || !data.hasSavedLocation)
            return;

        var playerAutoSave = FindFirstObjectByType<PlayerAutoSaveController>();

        if (playerAutoSave == null)
        {
            Debug.LogWarning("No PlayerAutoSaveController found in loaded scene.");
            return;
        }

        playerAutoSave.RestorePosition(data.position, data.rotation);
    }
}