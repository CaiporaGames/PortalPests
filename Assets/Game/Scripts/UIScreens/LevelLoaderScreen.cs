using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoaderScreen : BaseUIController
{
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform contentArea;
    private List<LevelIndexItem> levels;
    private IContentService contentService;
    private IHttpService http;
    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        contentService = ServiceLocator.Resolve<IContentService>();
        http = ServiceLocator.Resolve<IHttpService>();
           await LoadLevelIndexAsync();
    }

    private async UniTask LoadLevelIndexAsync()
    {
        var manifest = await contentService.LoadManifestAsync();
        levels = manifest.levels;
        Debug.Log($"Loaded manifest v{manifest.version}, levels: {levels.Count}");
        SpawnLevelButtons();
    }

    public override async UniTask ShowAsync<T>(T data = default)
    {
        await base.ShowAsync(data);
        contentArea.gameObject.SetActive(true);
    }

    public override async UniTask HideAsync<T>(T data = default)
    {
        await base.HideAsync(data);
        contentArea.gameObject.SetActive(false);
    }

    void SpawnLevelButtons()
    {
        if (levels == null) return;
        foreach (var lvl in levels)
        {
            var btnGO = Instantiate(levelButtonPrefab, contentArea);
            btnGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(lvl.pos.x, lvl.pos.y);

            // OPTIONAL: load thumbnail async
            this.RunThumbnailLoad(btnGO, lvl.thumbnailUrl).Forget();

            btnGO.GetComponent<Button>().onClick.AddListener(async () =>
            {
                await ServiceLocator.Resolve<IUIService>()
                    .ShowScreenAsync<object>(ScreenTypes.LevelDetailScreen, null, lvl);
            });
        }
    }

    private async UniTaskVoid RunThumbnailLoad(GameObject btnGO, string url)
    {
        try
        {
            var tex = await NetImg.GetTextureViaBytesAsync(url);
            var img = btnGO.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                img.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Thumb load failed: {url}\n{e.Message}");
        }
    }

}
