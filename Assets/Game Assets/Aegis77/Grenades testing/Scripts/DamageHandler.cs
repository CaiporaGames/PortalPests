using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(PersistentWorldObjectIdentity))]
public class DamageHandler : MonoBehaviour
{
    [SerializeField] private float health = 5f;
    [SerializeField] private GameObject prefabVisuals;
    [SerializeField] private AudioClip destroySFX;
    private PersistentWorldObjectIdentity _identity;
    private PersistentWorldStateManager _worldStateManager;
    private bool _isDestroyed;
    private AudioSource _audioSource;

    private void Awake()
    {
        _identity = GetComponent<PersistentWorldObjectIdentity>();
        _audioSource = GetComponent<AudioSource>();
        EventBus.Subscribe<Boolean>(EventType.InitializeMethods, Initialize);
    }

    private void Initialize(bool dummy)
    {
        _worldStateManager = ServiceLocator.Resolve<PersistentWorldStateManager>();
    }

    public void ApplyDamage(float dam)
    {
        health -= dam;
        if (health <= 0) DestroyCrateAsync().Forget();    
    }


    public async UniTask DestroyCrateAsync()
    {
        if (_isDestroyed) return;

        _isDestroyed = true;

        if (!_worldStateManager.IsInitialized)
            await _worldStateManager.InitializeAsync();

        await _worldStateManager.MarkDestroyedAsync(_identity);

        prefabVisuals.SetActive(false);
        if (destroySFX != null && _audioSource != null)
        {
            _audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); // Slight variation
            _audioSource.PlayOneShot(destroySFX);
        }
    }
}

