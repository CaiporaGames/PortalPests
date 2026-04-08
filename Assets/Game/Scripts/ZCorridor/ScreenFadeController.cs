using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    private void Awake()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    public async UniTask FadeOutAsync(float duration)
    {
        await FadeAsync(0f, 1f, duration);
    }

    public async UniTask FadeInAsync(float duration)
    {
        await FadeAsync(1f, 0f, duration);
    }

    private async UniTask FadeAsync(float from, float to, float duration)
    {
        if (fadeImage == null)
            return;

        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            await UniTask.Yield();
        }

        c.a = to;
        fadeImage.color = c;
    }
}