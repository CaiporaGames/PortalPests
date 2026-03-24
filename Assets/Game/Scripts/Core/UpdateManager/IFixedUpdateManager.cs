using System;

public interface IFixedUpdateManager

{

void Register(Action<float> callback);

void Unregister(Action<float> callback);

}