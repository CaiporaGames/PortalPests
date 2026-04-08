using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTravelDirection travelDirection;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var manager = ServiceLocator.Resolve<SceneTransitionManager>();
        manager.TryTransitionAsync(travelDirection).Forget();
    }
}