using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IUIService
{
    /// <summary>Registers a controller under a key.</summary>
    void Register(ScreenTypes key, IUIController controller);

    /// <summary>Show a registered screen by key.</summary>
    UniTask ShowScreenAsync<T>(ScreenTypes key, List<ScreenTypes> keepOpen = null, T data = default);

    /// <summary>Hide a registered screen by key.</summary>
    UniTask HideScreenAsync<T>(ScreenTypes key, T data = default);
}
