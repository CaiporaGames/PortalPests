using UnityEngine;

public enum SceneTravelDirection
{
    Next,
    Previous
}

public class SceneTransitionSpawnPoint
{
    [SerializeField] private SceneTravelDirection arrivalDirection;

    public SceneTravelDirection ArrivalDirection => arrivalDirection;
}