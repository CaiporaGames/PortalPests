using Cysharp.Threading.Tasks;

public class MainMenuController : BaseUIController
{
    public override async UniTask InitializeAsync()
    {
        // Load data, e.g. player prefs, settings
        await UniTask.Delay(100);
        // setup buttons callbacks
    }
}

/*
 * Usage:
 * 1. Place UIService on a persistent GameObject (e.g. UIManager).
 * 2. Create concrete controllers inheriting BaseUIController for each screen.
 * 3. Register them in UIEntryPoint.
 * 4. Use IUIService to Show/Hide screens from any system (inject or FindObject).
 * 5. For data-driven or event-driven flows, trigger UI via EventBus:
 *      EventBus.TriggerEvent(EventType.ShowUI, "MainMenu");
 */
