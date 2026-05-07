using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PersistentWorldObjectIdentity))]
public class LevelExitPortalController : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private int requiredEnemyCount = 1;

    [Header("Animation")]
    [SerializeField] private Transform movingPart;
    [SerializeField] private Vector3 openedLocalPosition;
    [SerializeField] private float openDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;

    private PersistentWorldObjectIdentity _identity;
    private PersistentWorldStateManager _worldStateManager;

    private int _currentKilledCount;
    private bool _isOpen;
    private Vector3 _closedLocalPosition;

    private async void Awake()
    {
        _identity = GetComponent<PersistentWorldObjectIdentity>();
        _worldStateManager = ServiceLocator.Resolve<PersistentWorldStateManager>();

        if (movingPart != null)
            _closedLocalPosition = movingPart.localPosition;
    }

    private async void Start()
    {
        if (!_worldStateManager.IsInitialized)
            await _worldStateManager.InitializeAsync();

        if (_worldStateManager.IsActivated(_identity))
        {
            ForceOpenVisualState();
            _isOpen = true;
            return;
        }

        CountAlreadyDestroyedTargetsInScene();
        ApplyClosedVisualState();

        if (_currentKilledCount >= requiredEnemyCount)
        {
            await OpenPortalAsync(playSound: false);
        }

        EventBus.Subscribe<TargetDestroyedPayload>(EventType.TargetDestroyed, OnTargetDestroyed);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TargetDestroyedPayload>(EventType.TargetDestroyed, OnTargetDestroyed);
    }

    private void OnTargetDestroyed(TargetDestroyedPayload payload)
    {
        if (_isOpen)
            return;

        if (payload.sceneName != SceneManager.GetActiveScene().name)
            return;

        _currentKilledCount++;

        if (_currentKilledCount >= requiredEnemyCount)
        {
            OpenPortalAsync(playSound: true).Forget();
        }
    }

    private void CountAlreadyDestroyedTargetsInScene()
    {
        _currentKilledCount = 0;

        var allTargets = FindObjectsByType<Target>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int aliveCount = 0;
        int savedDestroyedCount = 0;

        foreach (var target in allTargets)
        {
            var id = target.GetComponent<PersistentWorldObjectIdentity>();

            if (id != null && _worldStateManager.IsDestroyed(id))
            {
                savedDestroyedCount++;
                continue;
            }

            if (target.gameObject.activeInHierarchy)
            {
                aliveCount++;
            }
        }

        int assumedDestroyedCount = Mathf.Max(
            0,
            requiredEnemyCount - aliveCount
        );

        _currentKilledCount = Mathf.Max(
            savedDestroyedCount,
            assumedDestroyedCount
        );
    }

    private async UniTask OpenPortalAsync(bool playSound)
    {
        if (_isOpen)
            return;

        _isOpen = true;

        if (playSound && audioSource != null && openClip != null)
        {
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.PlayOneShot(openClip);
        }

        await AnimateOpenAsync();

        ForceOpenVisualState();

        await _worldStateManager.MarkActivatedAsync(_identity);
    }

    private async UniTask AnimateOpenAsync()
    {
        if (movingPart == null)
            return;

        Vector3 start = movingPart.localPosition;
        Vector3 end = openedLocalPosition;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            movingPart.localPosition = Vector3.Lerp(start, end, t);
            await UniTask.Yield();
        }

        movingPart.localPosition = end;
    }

    private void ForceOpenVisualState()
    {
        if (movingPart != null)
            movingPart.localPosition = openedLocalPosition;
    }

    private void ApplyClosedVisualState()
    {
        if (movingPart != null)
            movingPart.localPosition = _closedLocalPosition;
    }
}