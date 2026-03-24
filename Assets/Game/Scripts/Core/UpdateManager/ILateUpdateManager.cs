using System;

public interface ILateUpdateManager
{
    void Register(Action<float> callback);
    void Unregister(Action<float> callback);
}