using Cysharp.Threading.Tasks;
using UnityEngine;

public class WorldWindowSlideDown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform window;
    [SerializeField] private AudioSource windowAudioSource;
    [SerializeField] private AudioSource audioSource;

    [Header("Slide")]
    [SerializeField] private Vector3 localSlideOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private bool disableAfterSlideDown = false;

    private Vector3 _startLocalPosition;
    private Vector3 _downLocalPosition;
    private bool _isDown;
    private bool _isAnimating;

    private void Awake()
    {
        if (window == null)
            window = transform;

        _startLocalPosition = window.localPosition;
        _downLocalPosition = _startLocalPosition + localSlideOffset;
    }

    public void SlideDown()
    {
        if (_isAnimating || _isDown)
            return;

        SlideTo(_downLocalPosition, true).Forget();
    }

    public void SlideUp()
    {
        if (_isAnimating || !_isDown)
            return;

        if (window != null)
            window.gameObject.SetActive(true);

        SlideTo(_startLocalPosition, false).Forget();
    }

    public void ToggleSlide()
    {
        audioSource.Play();
        if (_isDown)
            SlideUp();
        else
            SlideDown();
    }

    private async UniTaskVoid SlideTo(Vector3 targetLocalPosition, bool down)
    {
        if (window == null)
            return;

        _isAnimating = true;

        Vector3 start = window.localPosition;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            float easedT = t * t * (3f - 2f * t);
            window.localPosition = Vector3.Lerp(start, targetLocalPosition, easedT);

            await UniTask.Yield();
        }
        windowAudioSource.Play();
        window.localPosition = targetLocalPosition;
        _isDown = down;
        _isAnimating = false;

        if (down && disableAfterSlideDown)
            window.gameObject.SetActive(false);
    }
}