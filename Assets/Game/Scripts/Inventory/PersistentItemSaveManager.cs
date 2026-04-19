using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentItemSaveManager : MonoBehaviour, IGameSystem
{
    private ISaveService _saveService;
    private PersistentItemsData _data;

    private readonly Dictionary<string, PersistentItemRecord> _lookup = new();

    public bool IsInitialized { get; private set; }


    public async UniTask InitializeAsync()
    {
        if (IsInitialized)
            return;
        _saveService = ServiceLocator.Resolve<ISaveService>();
        _data = await _saveService.LoadAsync<PersistentItemsData>(SaveType.PersistentItems);

        if (_data == null)
            _data = new PersistentItemsData();

        RebuildLookup();
        IsInitialized = true;
    }

    private void RebuildLookup()
    {
        _lookup.Clear();

        for (int i = 0; i < _data.items.Count; i++)
        {
            var record = _data.items[i];

            if (!string.IsNullOrEmpty(record.itemId))
                _lookup[record.itemId] = record;
        }
    }

    public IEnumerable<PersistentItemRecord> GetAllRecords()
    {
        return _data.items;
    }

    public PersistentItemRecord GetRecord(string itemId)
    {
        _lookup.TryGetValue(itemId, out var record);
        return record;
    }

    public PersistentItemRecord GetOrCreateRecord(PersistentItemIdentity identity, Transform itemTransform)
    {
        if (_lookup.TryGetValue(identity.ItemId, out var existing))
            return existing;

        var created = new PersistentItemRecord
        {
            itemId = identity.ItemId,
            itemType = identity.ItemType,
            state = PersistentItemState.InWorld,
            sceneName = SceneManager.GetActiveScene().name,
            slotIndex = -1,
            position = itemTransform.position,
            rotation = itemTransform.rotation,
            consumedByTargetId = ""
        };

        _data.items.Add(created);
        _lookup[created.itemId] = created;

        return created;
    }

    public async UniTask MarkInWorldAsync(PersistentItemIdentity identity, Transform itemTransform, string sceneName)
    {
        var record = GetOrCreateRecord(identity, itemTransform);

        record.state = PersistentItemState.InWorld;
        record.sceneName = sceneName;
        record.slotIndex = -1;
        record.position = itemTransform.position;
        record.rotation = itemTransform.rotation;
        record.consumedByTargetId = "";

        await SaveAsync();
    }

    public async UniTask MarkInInventoryAsync(PersistentItemIdentity identity, int slotIndex)
    {
        var record = GetOrCreateRecord(identity, identity.transform);

        record.state = PersistentItemState.InInventory;
        record.sceneName = "";
        record.slotIndex = slotIndex;
        record.consumedByTargetId = "";

        await SaveAsync();
    }

    public async UniTask MarkConsumedAsync(PersistentItemIdentity identity, string targetId = "")
    {
        var record = GetOrCreateRecord(identity, identity.transform);

        record.state = PersistentItemState.Consumed;
        record.sceneName = "";
        record.slotIndex = -1;
        record.consumedByTargetId = targetId ?? "";

        await SaveAsync();
    }

    public IEnumerable<PersistentItemRecord> GetInventoryItems()
    {
        return _data.items.Where(x => x.state == PersistentItemState.InInventory);
    }

    public async UniTask SaveAsync()
    {
        await _saveService.SaveAsync(SaveType.PersistentItems, _data);
    }
}