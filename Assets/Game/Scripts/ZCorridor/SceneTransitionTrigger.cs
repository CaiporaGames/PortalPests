using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private SceneArrivalType targetArrivalType;
    [SerializeField]private SceneTransitionManager _sceneTransitionManager;
    public SceneArrivalType TargetArrivalType => targetArrivalType;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _sceneTransitionManager.TryTransitionAsync(targetSceneName, targetArrivalType).Forget();
    }
}