using UnityEngine;

public enum SceneArrivalType
{
    None,
    FromPrevious,
    FromNext
}

public class SceneTransitionSpawnPoint
{
    [SerializeField] private SceneArrivalType arrivalType;

    public SceneArrivalType ArrivalType => arrivalType;
}