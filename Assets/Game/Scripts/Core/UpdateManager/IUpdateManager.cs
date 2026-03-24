using System;

public interface IUpdateManager
{
    /// Subscribe a callback that takes deltaTime
    void Register(Action<float> callback);

    /// Unsubscribe when you no longer need updates
    void Unregister(Action<float> callback);
}
