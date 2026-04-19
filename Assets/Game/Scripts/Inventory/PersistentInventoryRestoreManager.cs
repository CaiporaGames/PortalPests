using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentInventoryRestoreManager : MonoBehaviour
{
    [SerializeField] private PersistentItemPrefabDatabase prefabDatabase;

    private PersistentItemSaveManager _saveManager;
    private HipInventory _hipInventory;

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
        TryRestoreForCurrentScene();
    }

    private void TryRestoreForCurrentScene()
    {
        _hipInventory = FindFirstObjectByType<HipInventory>();

        if (_hipInventory == null)
        {
            Debug.LogWarning("No HipInventory found in current scene.");
            return;
        }

        RestoreInventoryItems();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        await UniTask.Yield();

        _hipInventory = FindFirstObjectByType<HipInventory>();

        if (_hipInventory == null)
        {
            Debug.LogWarning($"No HipInventory found in scene {scene.name}");
            return;
        }

        RestoreInventoryItems();
    }

    private void RestoreInventoryItems()
{
    if (_hipInventory == null)
    {
        Debug.LogError("HipInventory reference missing.");
        return;
    }

    if (prefabDatabase == null)
    {
        Debug.LogError("PersistentItemPrefabDatabase reference missing.");
        return;
    }


    ClearExistingRuntimeInventoryItems();

    IEnumerable<PersistentItemRecord> inventoryItems = _saveManager.GetInventoryItems();

    foreach (var record in inventoryItems)
    {
        InventorySlot slot = _hipInventory.GetSlotByIndex(record.slotIndex);
        if (slot == null)
        {
            Debug.LogWarning($"No slot found for slotIndex {record.slotIndex}");
            continue;
        }

        if (slot.IsOccupied)
        {
            Debug.LogWarning($"Slot {record.slotIndex} already occupied");
            continue;
        }

        PersistentPickableItem prefab = prefabDatabase.GetPrefab(record.itemType);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for item type {record.itemType}");
            continue;
        }


        PersistentPickableItem instance = Instantiate(prefab);
        instance.MarkAsRuntimeInventoryInstance();

        var identity = instance.GetComponent<PersistentItemIdentity>();
        if (identity != null)
            identity.SetRuntimeItemId(record.itemId);

        instance.gameObject.SetActive(true);
        instance.RestoreToSlot(slot);
    }
}

    private void ClearExistingRuntimeInventoryItems()
    {
        if (_hipInventory == null)
            return;

        var slots = _hipInventory.Slots;
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null)
                continue;

            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem.gameObject);
                slot.Clear();
            }
        }
    }
}