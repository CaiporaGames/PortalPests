using UnityEngine;

public enum SceneArrivalType
{
    None,
    FromPrevious,
    FromNext
}

public class SceneTransitionSpawnPoint : MonoBehaviour
{
    [SerializeField] private SceneArrivalType arrivalType;

    public SceneArrivalType ArrivalType => arrivalType;
}