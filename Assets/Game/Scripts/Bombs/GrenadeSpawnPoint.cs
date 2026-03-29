using System.Collections;
using UnityEngine;

public class GrenadeSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float respawnDelay = 5f;

    private GameObject currentGrenade;
    private bool isRespawning;

    private void Awake()
    {
        EventBus.Subscribe<GameObject>(EventType.GrenadePickedUp, NotifyGrenadePickedUp);
    }

    private void Start()
    {
        SpawnGrenade();
    }

    private void SpawnGrenade()
    {
        if (currentGrenade != null) return;

        currentGrenade = Instantiate(grenadePrefab, transform.position, transform.rotation);
    }

    public void NotifyGrenadePickedUp(GameObject pickedGrenade)
    {
        if (pickedGrenade != currentGrenade)
            return;

        currentGrenade = null;

        if (!isRespawning)
            StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;

        yield return new WaitForSeconds(respawnDelay);

        if (currentGrenade == null)
        {
            SpawnGrenade();
        }

        isRespawning = false;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameObject>(EventType.GrenadePickedUp, NotifyGrenadePickedUp);
    }
}