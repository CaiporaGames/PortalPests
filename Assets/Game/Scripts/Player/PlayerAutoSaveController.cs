using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerAutoSaveController : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private CharacterController characterController;

    private PlayerLocationSaveManager _locationSaveManager;

    public Transform PlayerRoot => playerRoot;

    private async void Start()
    {
        _locationSaveManager = ServiceLocator.Resolve<PlayerLocationSaveManager>();

        if (_locationSaveManager != null && !_locationSaveManager.IsInitialized)
            await _locationSaveManager.InitializeAsync();

        AutoSaveLoopAsync().Forget();
    }

    public void RestorePosition(Vector3 position, Quaternion rotation)
    {
        if (playerRoot == null)
            return;

        if (characterController != null)
            characterController.enabled = false;

        playerRoot.SetPositionAndRotation(position, rotation);

        if (characterController != null)
            characterController.enabled = true;
    }

    public async UniTask SaveNow()
    {
        if (_locationSaveManager == null || playerRoot == null)
            return;

        await _locationSaveManager.SavePlayerLocationAsync(playerRoot);
    }

    private async UniTaskVoid AutoSaveLoopAsync()
    {
        while (this != null && gameObject != null)
        {
            await UniTask.Delay(10000);

            if (this == null || gameObject == null)
                return;

            await SaveNow();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveNow().Forget();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveNow().Forget();
    }

    private void OnApplicationQuit()
    {
        SaveNow().Forget();
    }
}