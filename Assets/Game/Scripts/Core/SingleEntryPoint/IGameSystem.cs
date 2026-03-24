using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
public interface IGameSystem
{
     /// Heavy, one-shot boot logic (textures, DB look-ups, etc.)
    UniTask InitializeAsync();

    /// Called after every other system has finished and the
    /// local player object already exists (optional).
    //UniTask PostInitializeAsync();
}