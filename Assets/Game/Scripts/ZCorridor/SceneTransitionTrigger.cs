using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private SceneArrivalType targetArrivalType;
    private SceneTransitionManager _sceneTransitionManager;
    public SceneArrivalType TargetArrivalType => targetArrivalType;

    private void Awake()
    {
        _sceneTransitionManager = GetComponentInParent<SceneTransitionManager>();
        if (_sceneTransitionManager == null)
            Debug.LogError("SceneTransitionManager not found in the scene. Please ensure there is one present and it is properly initialized.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _sceneTransitionManager.TryTransitionAsync(targetSceneName, targetArrivalType).Forget();
    }
}