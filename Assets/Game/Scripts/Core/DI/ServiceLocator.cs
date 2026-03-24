using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, Func<object>> _transients = new();
    private static readonly Dictionary<Type, object> _singletons = new();

    public static void RegisterSingleton<T>(T instance)
    {
        var type = typeof(T);
        if (_singletons.ContainsKey(type))
            throw new ArgumentException($"Singleton for '{type}' already registered.");
        _singletons[type] = instance;
    }

    public static void RegisterTransient<T>(Func<T> factory)
    {
        var type = typeof(T);
        if (_transients.ContainsKey(type))
            throw new ArgumentException($"Transient for '{type}' already registered.");
        _transients[type] = () => factory();
    }

    public static T Resolve<T>()
    {
        var type = typeof(T);
        if (_singletons.TryGetValue(type, out var instance))
            return (T)instance;
        if (_transients.TryGetValue(type, out var factory))
            return (T)factory();
        throw new InvalidOperationException($"Service '{type}' not registered.");
    }
}