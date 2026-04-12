using Cysharp.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

//which target died, in which scene it happened
public struct TargetDestroyedPayload
{
    public string targetId;
    public string sceneName;
}

[RequireComponent(typeof(PersistentWorldObjectIdentity))]
public class Target : MonoBehaviour
{
    public float health = 5.0f;

    public ParticleSystem DestroyedEffect;

    [Header("Audio")]
    public RandomPlayer HitPlayer;
    private AudioSource _audioSource;

    public bool Destroyed => m_Destroyed;

    bool m_Destroyed = false;
    float m_CurrentHealth;

    private PersistentWorldObjectIdentity _identity;
    private PersistentWorldStateManager _worldStateManager;

    void Awake()
    {
        Helpers.RecursiveLayerChange(transform, LayerMask.NameToLayer("Target"));

        _identity = GetComponent<PersistentWorldObjectIdentity>();
        _worldStateManager = ServiceLocator.Resolve<PersistentWorldStateManager>();
    }

    async void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (!_worldStateManager.IsInitialized)
            await _worldStateManager.InitializeAsync();

        if (_worldStateManager.IsDestroyed(_identity))
        {
            m_Destroyed = true;
            gameObject.SetActive(false);
            return;
        }
        //TODO: maybe we need to handle the destroy effect
      /*   if (DestroyedEffect)
            PoolSystem.Instance.InitPool(DestroyedEffect, 16); */

        m_CurrentHealth = health;

        if (_audioSource != null && _audioSource.clip != null)
            _audioSource.time = Random.Range(0.0f, _audioSource.clip.length);
    }

    public void Got(float damage)
    {
        if (m_Destroyed)
            return;

        m_CurrentHealth -= damage;

        if (HitPlayer != null)
            HitPlayer.PlayRandom();

        if (m_CurrentHealth > 0)
            return;

        HandleDestroyedAsync().Forget();
    }

    private async UniTask HandleDestroyedAsync()
    {
        if (m_Destroyed)
            return;

        Vector3 position = transform.position;

        if (HitPlayer != null)
        {
            _audioSource.pitch = HitPlayer.source.pitch;
            _audioSource.PlayOneShot(HitPlayer.GetRandomClip());
        }

        if (DestroyedEffect != null)
        {
            //var effect = PoolSystem.Instance.GetInstance<ParticleSystem>(DestroyedEffect);
            DestroyedEffect.time = 0.0f;
            DestroyedEffect.transform.position = position;
            DestroyedEffect.Play();
        }

        m_Destroyed = true;

        if (!_worldStateManager.IsInitialized)
            await _worldStateManager.InitializeAsync();

        await _worldStateManager.MarkDestroyedAsync(_identity);
        
        EventBus.TriggerEvent(EventType.TargetDestroyed,
            new TargetDestroyedPayload
            {
                targetId = _identity.ObjectId,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            }
        );

        gameObject.SetActive(false);
    }
}