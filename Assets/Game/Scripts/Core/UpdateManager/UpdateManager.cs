using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour, IUpdateManager
{
    // Use List<T> for cache-friendly iteration
    private readonly List<Action<float>> _subscribers = new();

    public void Register(Action<float> callback)
    {
        if (!_subscribers.Contains(callback))
            _subscribers.Add(callback);
    }

    public void Unregister(Action<float> callback)
    {
        _subscribers.Remove(callback);
    }

    // Only one native→managed boundary per frame
    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0, n = _subscribers.Count; i < n; i++)
            _subscribers[i].Invoke(dt);
    }
}
