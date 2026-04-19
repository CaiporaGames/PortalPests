using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentWorldItemRestoreManager : MonoBehaviour
{
    [SerializeField] private PersistentItemPrefabDatabase prefabDatabase;

    private PersistentItemSaveManager _saveManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        _saveManager = ServiceLocator.Resolve<PersistentItemSaveManager>();

        if (_saveManager == null)
        {
            Debug.LogError("PersistentItemSaveManager not found.");
            return;
        }

        if (!_saveManager.IsInitialized)
            await _saveManager.InitializeAsync();

        SceneManager.sceneLoaded += OnSceneLoaded;

        await UniTask.Yield();
        RestoreWorldItemsForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        await UniTask.Yield();
        RestoreWorldItemsForCurrentScene();
    }

    private void RestoreWorldItemsForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        var existingIds = new HashSet<string>(
            FindObjectsByType<PersistentItemIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Select(x => x.ItemId)
        );

        var worldRecords = _saveManager.GetAllRecords()
            .Where(x => x.state == PersistentItemState.InWorld && x.sceneName == currentScene);

        foreach (var record in worldRecords)
        {
            if (existingIds.Contains(record.itemId))
                continue;

            var prefab = prefabDatabase.GetPrefab(record.itemType);
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab found for item type {record.itemType}");
                continue;
            }

            var instance = Instantiate(prefab, record.position, record.rotation);
            var identity = instance.GetComponent<PersistentItemIdentity>();
            if (identity != null)
                identity.SetRuntimeItemId(record.itemId);

            instance.MarkAsRuntimeWorldInstance();
            instance.gameObject.SetActive(true);
        }
    }
}