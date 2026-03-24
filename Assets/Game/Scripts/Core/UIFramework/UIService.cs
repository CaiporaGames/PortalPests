using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages lifecycle of all UI controllers
/// </summary>
public class UIService : MonoBehaviour, IUIService
{
    private readonly Dictionary<ScreenTypes, IUIController> _controllers = new();
    private readonly HashSet<ScreenTypes> _initialized = new();

    public async UniTask HideScreenAsync<T>(ScreenTypes key, T data = default)
    {
        var controller = _controllers[key];
        await controller.HideAsync(data);
    }

    public void Register(ScreenTypes key, IUIController controller)
    {
        if (_controllers.ContainsKey(key))
            throw new ArgumentException($"Controller with key '{key}' already registered.");

        _controllers[key] = controller;
    }

    public async UniTask ShowScreenAsync<T>(ScreenTypes key, List<ScreenTypes> keepOpen = null, T data = default)
    {

        keepOpen ??= new List<ScreenTypes>();

        foreach (var kvp in _controllers)
        {
            var screenKey = kvp.Key;
            var controller = kvp.Value;

            // Only hide other screens
            if (screenKey != key && !keepOpen.Contains(screenKey))
            {
                await controller.HideAsync(data);
            }
        }

        // Show the requested screen
        if (_controllers.TryGetValue(key, out var targetController))
        {
            if (!_initialized.Contains(key))
            {
                await targetController.InitializeAsync();
                _initialized.Add(key);
            }

            await targetController.ShowAsync(data);
        }
        else
        {
            throw new KeyNotFoundException($"No UI controller for key '{key}'.");
        }
    }

}