using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
//This is the Unity‑native, zero‑overhead, built‑in way to store temporary state.
// It is a static class with static fields, so it will persist across scene loads and be accessible from anywhere. 
//We use it to store the pending arrival type during scene transitions,
// so that the spawn point in the new scene can read it and position the player accordingly. 
//This avoids the need for more complex state management solutions or passing data through scene parameters.
public static class SceneTransitionData
{
    public static SceneArrivalType PendingArrivalType;
}
//In all the scenes we have two corridors. So, in each we need to enable the next or previous trigger and also put each corresponding
// one in the corresponding corridor.

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Player References")]
    [SerializeField] private Transform xrOriginRoot;
    [SerializeField] private Transform xrCamera;
    [SerializeField] private CharacterController characterController;
    [Header("Spawn Points")]
    [SerializeField] private GameObject[] _spawnPoints;

    //[Header("Optional Locomotion Control")]
    //[SerializeField] private MonoBehaviour[] locomotionBehavioursToDisable;

    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    private async void Awake()
    {
        var scene = SceneManager.GetActiveScene();

        if (scene.buildIndex == 0) // or compare by name
            return;

        await UniTask.Yield();
        MovePlayerToSpawnVR(SceneTransitionData.PendingArrivalType);

        var fade = FindFirstObjectByType<ScreenFadeController>();
        if (fade != null)
            await fade.FadeInAsync(fadeDuration);
    }

    public async UniTask TryTransitionAsync(string targetSceneName, SceneArrivalType targetArrivalType)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;

        var fade = FindFirstObjectByType<ScreenFadeController>();
        if (fade != null)
            await fade.FadeOutAsync(fadeDuration);

        SceneTransitionData.PendingArrivalType = targetArrivalType;

        await SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);

        _isTransitioning = false;
    }

    private void MovePlayerToSpawnVR(SceneArrivalType arrivalType)
    {

        GameObject targetSpawn = null;

        foreach (var spawn in _spawnPoints)
        {
            var spawnPoint = spawn.GetComponentInParent<SceneTransitionTrigger>();
            if (spawnPoint.TargetArrivalType == arrivalType)
            {
                targetSpawn = spawn;
                break;
            }
        }

        if (targetSpawn == null)
        {
            Debug.LogWarning($"No spawn point found for arrival type '{arrivalType}'.");
            return;
        }

        if (xrOriginRoot == null || xrCamera == null)
        {
            Debug.LogError("SceneTransitionManager: XR Origin Root or XR Camera is missing.");
            return;
        }

        Vector3 cameraOffset = xrCamera.position - xrOriginRoot.position;
        cameraOffset.y = 0f;

        Vector3 targetPosition = targetSpawn.transform.position - cameraOffset;

        Quaternion targetRotation = targetSpawn.transform.rotation;

        bool hadCharacterController = characterController != null && characterController.enabled;
        if (hadCharacterController)
            characterController.enabled = false;

        // Rotate origin so player faces the intended spawn forward
        Vector3 currentForward = xrCamera.forward;
        currentForward.y = 0f;

        Vector3 targetForward = targetSpawn.transform.forward;
        targetForward.y = 0f;

        if (currentForward.sqrMagnitude > 0.0001f && targetForward.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(currentForward, targetForward, Vector3.up);
            xrOriginRoot.Rotate(Vector3.up, angle);
        }

        // Recompute offset after rotation
        cameraOffset = xrCamera.position - xrOriginRoot.position;
        cameraOffset.y = 0f;

        targetPosition = targetSpawn.transform.position - cameraOffset;
        xrOriginRoot.position = targetPosition;

        if (hadCharacterController)
            characterController.enabled = true;
    }

    public void OnSceneLoaded()
    {
        Destroy(gameObject);
    }

  /*   private void SetLocomotionEnabled(bool enabled)
    {
        if (locomotionBehavioursToDisable == null)
            return;

        for (int i = 0; i < locomotionBehavioursToDisable.Length; i++)
        {
            if (locomotionBehavioursToDisable[i] != null)
                locomotionBehavioursToDisable[i].enabled = enabled;
        }
    } */
}