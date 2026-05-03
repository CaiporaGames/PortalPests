using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusicVibeController : MonoBehaviour
{
    public static MusicVibeController Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource combatSource;

    [Header("Settings")]
    [SerializeField] private float ambientVolume = 0.7f;
    [SerializeField] private float combatVolume = 0.8f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float returnToAmbientDelay = 6f;

    [Header("Enemy Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float enemyDetectionRadius = 8f;
    [SerializeField] private LayerMask enemyLayer;

    private MusicVibeState _currentState = MusicVibeState.Ambient;
    private float _lastCombatTriggerTime;
    private bool _isFading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupSources();
        SetStateImmediate(MusicVibeState.Ambient);
    }

    private void Update()
    {
        if (_currentState != MusicVibeState.Combat)
            return;

        bool hasNearbyEnemy = HasNearbyEnemy();
        bool recentlyTriggeredCombat = Time.time - _lastCombatTriggerTime <= returnToAmbientDelay;

        if (!hasNearbyEnemy && !recentlyTriggeredCombat)
        {
            SetMusicState(MusicVibeState.Ambient).Forget();
        }
    }

    public void TriggerCombat()
    {
        _lastCombatTriggerTime = Time.time;

        if (_currentState != MusicVibeState.Combat)
            SetMusicState(MusicVibeState.Combat).Forget();
    }

    public void RegisterPlayerAttack()
    {
        TriggerCombat();
    }

    public void RegisterEnemyAggro()
    {
        TriggerCombat();
    }

    private bool HasNearbyEnemy()
    {
        if (player == null)
            return false;

        Collider[] hits = Physics.OverlapSphere(
            player.position,
            enemyDetectionRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        return hits.Length > 0;
    }

    private async UniTaskVoid SetMusicState(MusicVibeState targetState)
    {
        if (_isFading || _currentState == targetState)
            return;

        _isFading = true;

        AudioSource fromSource = _currentState == MusicVibeState.Ambient
            ? ambientSource
            : combatSource;

        AudioSource toSource = targetState == MusicVibeState.Ambient
            ? ambientSource
            : combatSource;

        float toTargetVolume = targetState == MusicVibeState.Ambient
            ? ambientVolume
            : combatVolume;

        await CrossFade(fromSource, toSource, toTargetVolume);

        _currentState = targetState;
        _isFading = false;
    }

    private async UniTask CrossFade(AudioSource fromSource, AudioSource toSource, float toTargetVolume)
    {
        if (fromSource == null || toSource == null)
            return;

        if (!toSource.isPlaying)
            toSource.Play();

        float fromStartVolume = fromSource.volume;
        float toStartVolume = toSource.volume;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            fromSource.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            toSource.volume = Mathf.Lerp(toStartVolume, toTargetVolume, t);

            await UniTask.Yield();
        }

        fromSource.volume = 0f;
        toSource.volume = toTargetVolume;

        if (fromSource != toSource)
            fromSource.Pause();
    }

    private void SetupSources()
    {
        if (ambientSource != null)
        {
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        if (combatSource != null)
        {
            combatSource.loop = true;
            combatSource.playOnAwake = false;
        }
    }

    private void SetStateImmediate(MusicVibeState state)
    {
        _currentState = state;

        if (ambientSource != null)
        {
            ambientSource.volume = state == MusicVibeState.Ambient ? ambientVolume : 0f;

            if (state == MusicVibeState.Ambient)
                ambientSource.Play();
            else
                ambientSource.Pause();
        }

        if (combatSource != null)
        {
            combatSource.volume = state == MusicVibeState.Combat ? combatVolume : 0f;

            if (state == MusicVibeState.Combat)
                combatSource.Play();
            else
                combatSource.Pause();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Gizmos.DrawWireSphere(player.position, enemyDetectionRadius);
    }
}

public enum MusicVibeState
{
    Ambient,
    Combat
}