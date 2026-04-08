using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(PersistentWorldObjectIdentity))]
public class PersistentDestructibleLoader : MonoBehaviour
{
    private PersistentWorldObjectIdentity _identity;
    private PersistentWorldStateManager _worldStateManager;

    private async void Start()
    {
        _identity = GetComponent<PersistentWorldObjectIdentity>();
        _worldStateManager = ServiceLocator.Resolve<PersistentWorldStateManager>();

        if (!_worldStateManager.IsInitialized)
            await _worldStateManager.InitializeAsync();

        if (_worldStateManager.IsDestroyed(_identity))
        {
            gameObject.SetActive(false);
        }
    }
}