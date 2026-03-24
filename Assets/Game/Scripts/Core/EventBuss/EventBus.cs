using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<EventType, Delegate> _events = new();

    public static void Subscribe<T>(EventType eventType, Action<T> listener)
    {
        if (!_events.ContainsKey(eventType))
            _events[eventType] = null;

        _events[eventType] = (Action<T>)_events[eventType] + listener;
    }

    public static void Unsubscribe<T>(EventType eventType, Action<T> listener)
    {
        if (_events.ContainsKey(eventType))
        {
            _events[eventType] = (Action<T>)_events[eventType] - listener;
        }
    }

    public static void TriggerEvent<T>(EventType eventType, T param)
    {
        if (_events.TryGetValue(eventType, out var del) && del is Action<T> action)
        {
            action.Invoke(param);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"No listeners for {eventType} or mismatched type.");
        }
    }
}