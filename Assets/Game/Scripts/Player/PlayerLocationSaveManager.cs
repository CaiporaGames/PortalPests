using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLocationSaveManager : MonoBehaviour, IGameSystem
{
    private ISaveService _saveService;
    private PlayerLocationData _data;

    public bool IsInitialized { get; private set; }

    public async UniTask InitializeAsync()
    {
        if (IsInitialized)
            return;

        _saveService = ServiceLocator.Resolve<ISaveService>();
        _data = await _saveService.LoadAsync<PlayerLocationData>(SaveType.PlayerLocation);

        if (_data == null)
            _data = new PlayerLocationData();

        IsInitialized = true;
    }

    public PlayerLocationData GetData()
    {
        return _data;
    }

    public async UniTask SavePlayerLocationAsync(Transform playerRoot)
    {
        if (playerRoot == null)
            return;

        _data.hasSavedLocation = true;
        _data.sceneName = SceneManager.GetActiveScene().name;
        _data.position = playerRoot.position;
        _data.rotation = playerRoot.rotation;

        await _saveService.SaveAsync(SaveType.PlayerLocation, _data);
    }
}