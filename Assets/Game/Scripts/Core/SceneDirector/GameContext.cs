// A tiny context you pass to scene flows (so they don’t depend on singletons)
public readonly struct GameContext
{
    public readonly IUIService UI;
    public readonly IAuthService Auth;
    public readonly ISaveService Save;
    public readonly IContentService Content;
    public readonly IUpdateManager Update;
    public readonly SceneTypes CurrentSceneType;

    public GameContext(IUIService ui, IAuthService auth, ISaveService save, IContentService content, IUpdateManager update, SceneTypes currentSceneType = SceneTypes.MainScene)
    {
        UI = ui; Auth = auth; Save = save; Content = content; Update = update;
        CurrentSceneType = currentSceneType;
    }

    public static GameContext FromLocator() => new GameContext(
        ServiceLocator.Resolve<IUIService>(),
        ServiceLocator.Resolve<IAuthService>(),
        ServiceLocator.Resolve<ISaveService>(),
        ServiceLocator.Resolve<IContentService>(),
        ServiceLocator.Resolve<IUpdateManager>()
    );
}