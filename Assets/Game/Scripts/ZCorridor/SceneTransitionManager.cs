using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
//In all the scenes we have two corridors. So, in each we need to enable the next or previous trigger and also put each corresponding
// one in the corresponding corridor.
public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private CharacterController player;
    [SerializeField] private GameObject[] spawnPoints;
    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    public async UniTask TryTransitionAsync(SceneTravelDirection direction)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;

        string currentSceneName = SceneManager.GetActiveScene().name;
        string targetSceneName = GetTargetSceneName(currentSceneName, direction);

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"No valid target scene found from '{currentSceneName}' going '{direction}'.");
            _isTransitioning = false;
            return;
        }

        var fade = FindFirstObjectByType<ScreenFadeController>();
        if (fade != null)
            await fade.FadeOutAsync(fadeDuration);

        await LoadSceneAndMovePlayerAsync(targetSceneName, direction);

        if (fade != null)
            await fade.FadeInAsync(fadeDuration);

        _isTransitioning = false;
    }

    private string GetTargetSceneName(string currentSceneName, SceneTravelDirection direction)
    {
        Match match = Regex.Match(currentSceneName, @"^Level\s+(\d+)$");
        if (!match.Success)
        {
            Debug.LogError($"Scene name '{currentSceneName}' does not match expected format 'Level X'.");
            return null;
        }

        int levelNumber = int.Parse(match.Groups[1].Value);

        if (direction == SceneTravelDirection.Next)
            levelNumber++;
        else
            levelNumber--;

        if (levelNumber < 1)
            return null;

        return $"Level {levelNumber}";
    }

    private async UniTask LoadSceneAndMovePlayerAsync(string targetSceneName, SceneTravelDirection direction)
    {
        var loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);

        while (!loadOp.isDone)
            await UniTask.Yield();

        await UniTask.Yield();

        MovePlayerToSpawn(direction);
    }

    private void MovePlayerToSpawn(SceneTravelDirection direction)
    {
        SceneTravelDirection arrivalDirection =
            direction == SceneTravelDirection.Next
            ? SceneTravelDirection.Previous
            : SceneTravelDirection.Next;


        foreach (var spawn in spawnPoints)
        {
            if (spawn.GetComponent<SceneTransitionSpawnPoint>().ArrivalDirection != arrivalDirection) continue;

            bool wasEnabled = player.enabled;
            player.enabled = false;

            Transform root = player.transform;
            root.position = spawn.transform.position;
            root.rotation = spawn.transform.rotation;

            player.enabled = wasEnabled;
            return;
        }
    }
}