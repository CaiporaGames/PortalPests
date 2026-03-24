using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// Base class for MonoBehaviour-based screens with fade + enable/disable.
/// </summary>
public abstract class BaseUIController : MonoBehaviour, IUIController
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;

    [Header("Behavior")]
    [Tooltip("Seconds for the fade animation.")]
    public float fadeDuration = 0.25f;

    [Tooltip("Use unscaled time so fades run during pause menus etc.")]
    public bool useUnscaledTime = true;

    [Tooltip("Start hidden (alpha=0, not interactable). Optionally deactivate at start.")]
    public bool startHidden = true;

    [Tooltip("After Hide fades to 0, deactivate to remove all cost while hidden.")]
    public bool disableAfterHide = true;

    [Tooltip("If true, will enable/disable the Canvas component instead of the GameObject.")]
    public bool disableCanvasInsteadOfGO = false;

    private CancellationTokenSource _fadeCts;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (startHidden)
        {
            // Start invisible and non-interactive
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (disableAfterHide)
                SetActiveState(false);
        }
    }

    protected virtual void OnDestroy()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
    }

    public virtual async UniTask InitializeAsync()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask ShowAsync<T>(T data = default)
    {
        // Ensure we are active before fading in
        if (disableAfterHide && !IsActive())
            SetActiveState(true);

        await FadeTo(1f);
    }

    public virtual async UniTask HideAsync<T>(T data = default)
    {
        await FadeTo(0f);

        // After fully faded out, disable to remove rebuild/raycast costs
        if (disableAfterHide)
            SetActiveState(false);
    }

    // ---------- Helpers ----------

    private bool IsActive()
    {
        if (disableCanvasInsteadOfGO)
        {
            var c = GetComponent<Canvas>();
            if (c != null) return c.enabled;
            // Fallback if there's no Canvas on this object
            return gameObject.activeInHierarchy;
        }
        return gameObject.activeInHierarchy;
    }

    private void SetActiveState(bool active)
    {
        if (disableCanvasInsteadOfGO)
        {
            var c = GetComponent<Canvas>();
            if (c != null) { c.enabled = active; return; }
            // Fallback
            gameObject.SetActive(active);
            return;
        }

        gameObject.SetActive(active);
    }

    private async UniTask FadeTo(float target)
    {
        // Cancel any in-flight fade
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = new CancellationTokenSource();
        var token = _fadeCts.Token;

        float start = canvasGroup.alpha;

        // If already there, just set flags and exit
        if (Mathf.Approximately(start, target))
        {
            bool visible = target >= 1f;
            canvasGroup.alpha = target;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            return;
        }

        // Gate input during transitions
        if (target < 1f)
        {
            // Fading out: disable input immediately
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Fading in: block raycasts to prevent clicks leaking through,
            // but keep interactable OFF until fully visible
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.0001f, fadeDuration);

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(start, target, t);

            // Use Update loop timing (smoother for UI)
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        // Snap to final
        canvasGroup.alpha = target;
        bool nowVisible = target >= 1f;
        canvasGroup.interactable = nowVisible;
        canvasGroup.blocksRaycasts = nowVisible;
    }
}
