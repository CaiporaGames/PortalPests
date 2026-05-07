using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavedGameBootstrapSystem : MonoBehaviour, IGameSystem
{
    [SerializeField] private string defaultSceneName = "Level 1";

    private PlayerLocationSaveManager _locationSaveManager;
    private PlayerLocationData _cachedData;

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

        _cachedData = _locationSaveManager.GetData();

        IsInitialized = true;
    }

    public async void LoadSavedOrDefaultScene()
    {
        await LoadSavedOrDefaultSceneAsync();
    }

    public async UniTask LoadSavedOrDefaultSceneAsync()
    {
        if (!IsInitialized)
            await InitializeAsync();

        string sceneToLoad =
            _cachedData != null &&
            _cachedData.hasSavedLocation &&
            !string.IsNullOrWhiteSpace(_cachedData.sceneName)
                ? _cachedData.sceneName
                : defaultSceneName;

        await SceneManager.LoadSceneAsync(sceneToLoad);

        await UniTask.Yield();
        await UniTask.DelayFrame(1);

        RestorePlayerPosition(_cachedData);
    }

    private void RestorePlayerPosition(PlayerLocationData data)
    {
        if (data == null || !data.hasSavedLocation)
            return;

        if (string.IsNullOrWhiteSpace(data.sceneName))
            return;

        if (SceneManager.GetActiveScene().name != data.sceneName)
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