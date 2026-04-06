using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentWorldStateManager : MonoBehaviour, IGameSystem
{
    private ISaveService _saveService;
    private WorldStatesData _data;
    private readonly Dictionary<string, PersistentWorldObjectState> _lookup = new();

    public bool IsInitialized { get; private set; }

    public async UniTask InitializeAsync()
    {
        if (IsInitialized)
            return;

        _saveService = ServiceLocator.Resolve<ISaveService>();

        _data = await _saveService.LoadAsync<WorldStatesData>(SaveType.WorldStates);
        if (_data == null)
            _data = new WorldStatesData();

        RebuildLookup();
        IsInitialized = true;
    }

    private void RebuildLookup()
    {
        _lookup.Clear();

        foreach (var obj in _data.objects)
        {
            if (!string.IsNullOrEmpty(obj.objectId))
                _lookup[obj.objectId] = obj;
        }
    }

    public PersistentWorldObjectState GetOrCreateState(PersistentWorldObjectIdentity identity)
    {
        if (_lookup.TryGetValue(identity.ObjectId, out var existing))
            return existing;

        var created = new PersistentWorldObjectState
        {
            objectId = identity.ObjectId,
            sceneName = SceneManager.GetActiveScene().name,
            isDestroyed = false
        };

        _data.objects.Add(created);
        _lookup[created.objectId] = created;
        return created;
    }

    public bool IsDestroyed(PersistentWorldObjectIdentity identity)
    {
        var state = GetOrCreateState(identity);
        return state.isDestroyed;
    }

    public async UniTask MarkDestroyedAsync(PersistentWorldObjectIdentity identity)
    {
        var state = GetOrCreateState(identity);
        state.sceneName = SceneManager.GetActiveScene().name;
        state.isDestroyed = true;

        await SaveAsync();
    }

    public async UniTask SaveAsync()
    {
        await _saveService.SaveAsync(SaveType.WorldStates, _data);
    }
}