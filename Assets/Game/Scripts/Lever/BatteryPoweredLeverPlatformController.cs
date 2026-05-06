using Cysharp.Threading.Tasks;
using UnityEngine;

public class BatteryPoweredLeverPlatformController : MonoBehaviour
{
    [SerializeField] private AudioSource leverAudioSource;
    [Header("Platform")]
    [SerializeField] private Transform platform;
    [SerializeField] private Vector3 platformUpLocalPosition;
    [SerializeField] private Vector3 platformDownLocalPosition;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 1f;

    [Header("Battery Requirement")]
    [SerializeField] private bool requireBattery = true;

    private bool _batteryInstalled;
    private bool _leverDown;
    private bool _isMoving;

    private void Awake()
    {
        if (platform != null)
            platformUpLocalPosition = platform.localPosition;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<bool>(
            EventType.BatteryInstalledChanged,
            OnBatteryInstalledChanged
        );
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<bool>(
            EventType.BatteryInstalledChanged,
            OnBatteryInstalledChanged
        );
    }

    private void OnBatteryInstalledChanged(bool installed)
    {
        _batteryInstalled = installed;

        EvaluatePlatformState();
    }

    public void SetLeverDown()
    {
        _leverDown = true;
        EvaluatePlatformState();
    }

    public void SetLeverUp()
    {
        _leverDown = false;
        EvaluatePlatformState();
    }

    public void ToggleLever()
    {
        _leverDown = !_leverDown;
        EvaluatePlatformState();
    }

    private void EvaluatePlatformState()
    {
        if (platform == null)
            return;

        bool canMoveDown = !requireBattery || _batteryInstalled;
        bool shouldMoveDown = _leverDown && canMoveDown;

        Vector3 targetPosition = shouldMoveDown
            ? platformDownLocalPosition
            : platformUpLocalPosition;

        MovePlatform(targetPosition).Forget();
    }

    private async UniTaskVoid MovePlatform(Vector3 targetLocalPosition)
    {
        if (_isMoving)
            return;

        _isMoving = true;
        leverAudioSource.Play();
        Vector3 startPosition = platform.localPosition;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / moveDuration);
            float eased = SmoothStep(t);

            platform.localPosition = Vector3.Lerp(
                startPosition,
                targetLocalPosition,
                eased
            );

            await UniTask.Yield();
        }

        platform.localPosition = targetLocalPosition;

        _isMoving = false;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}