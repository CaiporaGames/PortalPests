using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRFloatingMenu : MonoBehaviour
{

    [Header("Quit")]
    [SerializeField] private string bootSceneName = "Boot";


    private PlayerAutoSaveController _autoSaveController;

    private void Start()
    {
        _autoSaveController = FindFirstObjectByType<PlayerAutoSaveController>();
    }

    public void SaveAndQuitToBoot()
    {
        SaveAndQuitToBootAsync().Forget();
    }

    private async UniTaskVoid SaveAndQuitToBootAsync()
    {
        Time.timeScale = 1f;

        if (_autoSaveController == null)
            _autoSaveController = FindFirstObjectByType<PlayerAutoSaveController>();

        if (_autoSaveController != null)
            await _autoSaveController.SaveNow();

        await SceneManager.LoadSceneAsync(bootSceneName);
    }

    public async void QuitGame()
    {
         if (_autoSaveController != null)
            await _autoSaveController.SaveNow();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}