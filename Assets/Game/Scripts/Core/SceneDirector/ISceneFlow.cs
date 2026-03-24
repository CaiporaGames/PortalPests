using Cysharp.Threading.Tasks;

public interface ISceneFlow
{
    UniTask InitializeAsync(GameContext ctx);
    UniTask ShutdownAsync(); // optional for cleanup, unregister temp services, etc.
}