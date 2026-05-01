using UnityEngine;

public class ControllerHeadMenuRay : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private LayerMask headSphereLayer;

    [Header("Menu")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private float openDelay = 0.15f;
    [SerializeField] private float closeDelay = 0.25f;

    private float _hoverTimer;
    private float _exitTimer;
    private bool _isPointingAtHead;

    private void Reset()
    {
        rayOrigin = transform;
    }

    private void Update()
    {
        bool hitHead = Physics.Raycast(
            rayOrigin.position,
            rayOrigin.forward,
            out RaycastHit hit,
            maxDistance,
            headSphereLayer,
            QueryTriggerInteraction.Collide
        );

        _isPointingAtHead = hitHead;

        if (hitHead)
        {
            _hoverTimer += Time.deltaTime;
            _exitTimer = 0f;

            if (_hoverTimer >= openDelay && menuCanvas != null && !menuCanvas.activeSelf)
                menuCanvas.SetActive(true);
        }
        else
        {
            _hoverTimer = 0f;
            _exitTimer += Time.deltaTime;

            if (_exitTimer >= closeDelay && menuCanvas != null && menuCanvas.activeSelf)
                menuCanvas.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = rayOrigin != null ? rayOrigin : transform;
        Gizmos.DrawRay(origin.position, origin.forward * maxDistance);
    }
}