using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;

public class StoryScreen : BaseUIController
{
    [SerializeField] private Image storyImage;
    [SerializeField] private TextMeshProUGUI japaneseLanguageText;
    [SerializeField] private TextMeshProUGUI romajiText;
    //[SerializeField] private Text otherLanguageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button returnButton;

    private List<StorySection> story;
    private int currentIndex = 0;
    private LevelData currentLevel;

    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        nextButton.onClick.AddListener(OnNext);
        backButton.onClick.AddListener(OnBack);
        returnButton.onClick.AddListener(async () =>
        {
            await ServiceLocator.Resolve<IUIService>().ShowScreenAsync<object>(ScreenTypes.LevelLoaderScreen);
        });
    }

    public override async UniTask ShowAsync<T>(T data = default)
    {
        await base.ShowAsync(data);
        if (data is LevelData level)
        {
            currentLevel = level;
            SetLevel(currentLevel);
            ShowSection();
        }
        else
        {
            Debug.LogError("Invalid data type for StoryScreen: " + data.GetType());
        }
    }

    public void SetLevel(LevelData level)
    {
        story = level.story;
        currentIndex = 0;
    }

    private async void ShowSection()
    {
        var section = story[currentIndex];

        // Set text fields
        japaneseLanguageText.text = section.text_jp;
        romajiText.text = section.text_romaji;
        //otherLanguageText.text = section.text_other; // if you have another language

        // Load story image
        if (!string.IsNullOrEmpty(section.imageUrl))
        {
            try
            {
                var tex = await NetImg.GetTextureViaBytesAsync(section.imageUrl);
                var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                storyImage.sprite = sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load story image from {section.imageUrl}\n{e}");
                storyImage.sprite = null; // optional: fallback sprite
            }
        }
        else
        {
            storyImage.sprite = null; // no image for this section
        }
    }

    public void OnNext()
    {
        if (currentIndex < story.Count - 1)
        {
            currentIndex++;
            ShowSection();
        }
    }

    public void OnBack()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowSection();
        }
    }
}
