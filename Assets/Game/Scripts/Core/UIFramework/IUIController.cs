using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IUIController
{
    /// <summary>Heavy one-time setup (load assets, data binding).</summary>
    UniTask InitializeAsync();

    /// <summary>Show the screen with optional payload.</summary>
    UniTask ShowAsync<T>(T data = default);

    /// <summary>Hide the screen.</summary>
    UniTask HideAsync<T>(T data = default);
}