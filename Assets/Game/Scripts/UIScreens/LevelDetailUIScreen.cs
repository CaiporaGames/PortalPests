using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDetailUIScreen : BaseUIController
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Button startStoryButton;
    [SerializeField] private Button gameButton;
    [SerializeField] private Button returnButton;
    private LevelIndexItem currentLevel;
    private IContentService contentService;
    private IHttpService http;

    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        contentService = ServiceLocator.Resolve<IContentService>();
        http = ServiceLocator.Resolve<IHttpService>();
        startStoryButton.onClick.AddListener(OnStartPressed);
        returnButton.onClick.AddListener(async () =>
        {
            await ServiceLocator.Resolve<IUIService>().ShowScreenAsync<object>(ScreenTypes.LevelLoaderScreen);
        });

        gameButton.onClick.AddListener(OnStartGamePressed);
    }

    public override async UniTask ShowAsync<T>(T data = default)
    {
        await base.ShowAsync(data);
        if (data is LevelIndexItem lvl)
        {
            currentLevel = lvl;
            titleText.text = lvl.title;
            descriptionText.text = $"XP: {lvl.xpRequired}"; // or fetch a short desc later
            // thumbnail
            var tex = await NetImg.GetTextureViaBytesAsync(currentLevel.thumbnailUrl);
            thumbnailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        }
    }

    public async void OnStartPressed()
    {
        var story = await contentService.LoadStoryAsync(currentLevel.levelUrl);

        // Build LevelData if StoryScreen expects it
        var levelData = new LevelData
        {
            title = story.title,
            description = story.description,
            // map to your existing fields if needed:
            story = new List<StorySection>(story.story) // or convert to your type
        };

        await ServiceLocator.Resolve<IUIService>().ShowScreenAsync<object>(ScreenTypes.StoryScreen, null, levelData);
    }
    
    public async void OnStartGamePressed()
    {
        if (currentLevel == null) { Debug.LogWarning("No level selected."); return; }

        try
        {
            var director = ServiceLocator.Resolve<ISceneDirector>();
            var flow = await director.LoadAsync(
                SceneTypes.GameScene,
                this.GetCancellationTokenOnDestroy(),                            
                new Progress<float>(p => Debug.Log($"Loading {p*100f:0}%"))
            );

            if (flow == null) { Debug.LogError("No ISceneFlow found."); return; }

            var ctx = GameContext.FromLocator();                                  
            await flow.InitializeAsync(ctx);
        }
        catch (OperationCanceledException) { /* user backed out; tidy UI if needed */ }
    }

}
